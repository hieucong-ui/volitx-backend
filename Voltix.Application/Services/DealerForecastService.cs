using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dealer;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System.Security.Claims;

namespace Voltix.Application.Services
{
    public class DealerForecastService : IDealerForecastService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DealerForecastService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> BuildDailySnapshotAsync(DateTime utcDate, CancellationToken ct)
        {
            try
            {
                var dateOnly = utcDate.Date;
                var dateOnlyPrev = dateOnly.AddDays(-1);

                var pairs = await _unitOfWork.EVTemplateRepository.GetActiveDealerTemplatePairsAsync(ct);
                var openingMap = await _unitOfWork.DealerDailyInventoryRepository.GetClosingStockMapAsync(dateOnlyPrev, ct);
                var inflowMap = await _unitOfWork.ElectricVehicleRepository.GetInflowAsync(dateOnly, ct);
                var outflowMap = await _unitOfWork.ElectricVehicleRepository.GetOutflowAsync(dateOnly, ct);

                var maxSnapshotDate = await _unitOfWork.DealerDailyInventoryRepository.GetMaxSnapshotDateAsync(ct);

                IReadOnlyDictionary<(Guid DealerId, Guid EVTemplateId), int> openingBase;
                if (maxSnapshotDate is null)
                {
                    openingBase = await _unitOfWork.ElectricVehicleRepository.GetDealerOnHandStockAsync(ct);
                }
                else
                {
                    if (!openingMap.Any())
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = $"Missing snapshot for previous date {dateOnlyPrev:yyyy-MM-dd}. Please rebuild snapshots in chronological order."
                        };
                    }

                    openingBase = openingMap;
                }

                var rows = new List<DealerDailyInventory>();
                foreach (var (dealerId, templateId) in pairs)
                {
                    openingBase.TryGetValue((dealerId, templateId), out var opening);
                    inflowMap.TryGetValue((dealerId, templateId), out var inflow);
                    outflowMap.TryGetValue((dealerId, templateId), out var outflow);
                    var closing = opening + inflow - outflow;
                    if (closing < 0) closing = 0;

                    rows.Add(new DealerDailyInventory
                    {
                        DealerId = dealerId,
                        EVTemplateId = templateId,
                        SnapshotDate = dateOnly,
                        OpeningStock = opening,
                        Inflow = inflow,
                        Outflow = outflow,
                        ClosingStock = closing
                    });
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _unitOfWork.DealerDailyInventoryRepository.UpsertRangeAsync(rows, ct);
                    await _unitOfWork.SaveAsync(ct);
                }, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Daily snapshot built successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        Rows = rows,
                        Count = rows.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error building daily snapshot: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> EvaluateInventoryRiskAsync(int horizonDays, CancellationToken ct)
        {
            try
            {
                if (horizonDays <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "HorizonDays must be > 0"
                    };
                }

                var snapshotDate = await _unitOfWork.DealerDailyInventoryRepository.GetMaxSnapshotDateAsync(ct);
                if (snapshotDate is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No inventory snapshot data found."
                    };
                }

                snapshotDate = new DateTime(snapshotDate.Value.Year, snapshotDate.Value.Month, snapshotDate.Value.Day, 0, 0, 0, DateTimeKind.Utc);

                var latestInventories = await _unitOfWork.DealerDailyInventoryRepository.GetByDateAsync(snapshotDate.Value, ct);

                var latestMap = latestInventories.Where(x => x.ClosingStock > 0)
                    .ToDictionary(
                        x => (x.DealerId, x.EVTemplateId),
                        x => x.ClosingStock);

                if (!latestMap.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "No dealers have stock. Nothing to evaluate.",
                        Result = new
                        {
                            Count = 0
                        }
                    };
                }

                var from = snapshotDate.Value.AddDays(1);
                var to = snapshotDate.Value.AddDays(horizonDays);

                var forecasts = await _unitOfWork.DealerInventoryForecastRepository.GetForecastsInRangeAsync(from, to, ct);

                var forecastByPair = forecasts.GroupBy(f => new { f.DealerId, f.EVTemplateId })
                    .ToDictionary(
                        g => (g.Key.DealerId, g.Key.EVTemplateId),
                        g => g.OrderBy(x => x.TargetDate).ToList());

                var risks = new List<DealerInventoryRisk>();

                const int DefaultAlertThreshold = 10;
                const int CriticalThreshold = 0;
                const int MediumThreshold = 5;

                foreach (var kvp in latestMap)
                {
                    var key = kvp.Key;
                    var startingStock = kvp.Value;

                    if (!forecastByPair.TryGetValue(key, out var forecastSeries))
                        continue;

                    var current = startingStock;

                    foreach (var f in forecastSeries)
                    {
                        var demand = (int)Math.Round(f.Forecast);

                        current -= demand;

                        InventoryRiskLevel level = InventoryRiskLevel.None;

                        if (current <= CriticalThreshold)
                        {
                            level = InventoryRiskLevel.Critical;
                        }
                        else if (current <= MediumThreshold)
                        {
                            level = InventoryRiskLevel.Medium;
                        }
                        else if (current <= DefaultAlertThreshold)
                        {
                            level = InventoryRiskLevel.High;
                        }

                        if (level != InventoryRiskLevel.None)
                        {
                            risks.Add(new DealerInventoryRisk
                            {
                                Id = Guid.NewGuid(),
                                DealerId = key.DealerId,
                                EVTemplateId = key.EVTemplateId,
                                TargetDate = f.TargetDate,
                                ExpectedClosing = current,
                                RiskLevel = level,
                                CreatedAt = DateTime.UtcNow,
                                IsResolved = false
                            });

                            if (level == InventoryRiskLevel.Critical)
                                break;
                        }

                        if (current < 0) current = 0;
                    }
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _unitOfWork.DealerInventoryRiskRepository.UpsertRangeAsync(risks, ct);
                    await _unitOfWork.SaveAsync(ct);
                }, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Inventory risk evaluated successfully.",
                    Result = new
                    {
                        Count = risks.Count,
                        SnapshotDate = snapshotDate,
                        HorizonDays = horizonDays
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error evaluating inventory risk: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDemandSeriesAsync(ClaimsPrincipal userClaim, Guid? dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not login yet.",
                        StatusCode = 401
                    };
                }

                var role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (role is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found.",
                        StatusCode = 403
                    };
                }

                Guid effectiveDealerId;
                if (role.Equals(StaticUserRole.EVMStaff) || role.Equals(StaticUserRole.Admin))
                {
                    if (dealerId is null || dealerId == Guid.Empty)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "DealerId is required for Admin/EVMStaff or roles have permisstion to get demand series."
                        };
                    }

                    effectiveDealerId = dealerId.Value;
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found for the user.",
                            StatusCode = 404
                        };
                    }

                    effectiveDealerId = dealer.Id;
                }

                var fromDate = from.Date;
                var toDate = to.Date;

                var data = await _unitOfWork.DealerDailyInventoryRepository.GetRangeAsync(effectiveDealerId, evTemplateId, fromDate, toDate, ct);

                var getData = _mapper.Map<List<DemandSeriesPointDTO>>(data);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Demand series retrieved successfully.",
                    StatusCode = 200,
                    Result = getData
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving demand series: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpsertForecastBatchAsync(IEnumerable<UpsertDealerInventoryForecastDTO> forecasts, CancellationToken ct)
        {
            try
            {
                var now = DateTime.UtcNow;

                var rows = forecasts.Select(f => new DealerInventoryForecast
                {
                    Id = Guid.NewGuid(),
                    DealerId = f.DealerId,
                    EVTemplateId = f.EVTemplateId,
                    TargetDate = f.TargetDate.Date,
                    Forecast = f.Forecast,
                    ForecastLower = f.ForecastLower,
                    ForecastUpper = f.ForecastUpper,
                    CreatedAtUtc = now,
                    ModelVersion = f.ModelVersion
                }).ToList();

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _unitOfWork.DealerInventoryForecastRepository.UpsertRangeAsync(rows, ct);
                    await _unitOfWork.SaveAsync(ct);
                }, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Forecast batch upserted successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        Count = rows.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error upserting forecast batch: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetForecastTargetsAsync(CancellationToken ct)
        {
            try
            {
                var maxDate = await _unitOfWork.DealerDailyInventoryRepository.GetMaxSnapshotDateAsync(ct);

                if (maxDate is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "No snapshot data yet.",
                        Result = new
                        {
                            SnapshotDate = (DateTime?)null,
                            Count = 0,
                            Targets = new List<ForecastTargetDTO>()
                        }
                    };
                }

                var snapshotDate = maxDate.Value.Date;

                var inventories = await _unitOfWork.DealerDailyInventoryRepository.GetByDateAsync(snapshotDate, ct);

                var targets = inventories.Where(x => x.OpeningStock > 0 || x.ClosingStock > 0).ToList();

                var targetDTOs = _mapper.Map<List<ForecastTargetDTO>>(targets);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Forecast targets retrieved successfully.",
                    Result = new
                    {
                        SnapshotDate = snapshotDate,
                        Count = targets.Count,
                        Targets = targetDTOs
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error getting forecast targets: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetForecastSeriesAsync(ClaimsPrincipal userClaim, Guid? dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not login yet.",
                        StatusCode = 401
                    };
                }

                var role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (role is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found.",
                        StatusCode = 403
                    };
                }

                Guid effectiveDealerId;
                if (role.Equals(StaticUserRole.EVMStaff) || role.Equals(StaticUserRole.Admin))
                {
                    if (dealerId is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "DealerId is required for Admin/EVMStaff."
                        };
                    }

                    effectiveDealerId = dealerId.Value;
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                        if (dealer is null)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Dealer not found for the user.",
                                StatusCode = 404
                            };
                        }
                    }

                    effectiveDealerId = dealer.Id;
                }

                var fromDate = from.Date;
                var toDate = to.Date;

                if (fromDate > toDate)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "From date must be <= To date."
                    };
                }

                var forecastsInRange = await _unitOfWork.DealerInventoryForecastRepository.GetForecastsInRangeAsync(fromDate, toDate, ct);

                var filtered = forecastsInRange
                    .Where(f => f.DealerId == effectiveDealerId && f.EVTemplateId == evTemplateId)
                    .OrderBy(f => f.TargetDate)
                    .ToList();

                var dto = _mapper.Map<List<GetForecastSeriesPointDTO>>(filtered);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Forecast series retrieved successfully.",
                    Result = dto
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error retrieving forecast series: {ex.Message}"
                };
            }
        }
    }
}
