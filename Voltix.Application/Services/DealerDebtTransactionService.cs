using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerDebt;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class DealerDebtTransactionService : IDealerDebtTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DealerDebtTransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> CraeteDealerDebtTransaction(CreateDealerDebtTransactionDTO dealerDebtTransactionDTO, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(dealerDebtTransactionDTO.OccurredAtUtc, DateTimeKind.Utc);

                var duplicated = await _unitOfWork.DealerDebtTransactionRepository.IsDuplicated(dealerDebtTransactionDTO.DealerId, dealerDebtTransactionDTO.ExternalId, ct);
                if (duplicated)
                {
                    return new ResponseDTO
                    {
                        StatusCode = 409,
                        IsSuccess = true,
                        Message = "Duplicated transaction. No operation performed."
                    };
                }

                var transaction = new DealerDebtTransaction
                {
                    DealerId = dealerDebtTransactionDTO.DealerId,
                    OccurredAtUtc = now,
                    Type = dealerDebtTransactionDTO.Type,
                    Amount = dealerDebtTransactionDTO.Amount,
                    IsIncrease = dealerDebtTransactionDTO.Type == DealerDebtTransactionType.Adjustment && dealerDebtTransactionDTO.IsIncrease == true,
                    ExternalId = dealerDebtTransactionDTO.ExternalId!,
                    SourceType = dealerDebtTransactionDTO.SourceType,
                    SourceId = dealerDebtTransactionDTO.SourceId,
                    SourceNo = dealerDebtTransactionDTO.SourceNo,
                    Method = dealerDebtTransactionDTO.Method,
                    ReferenceNo = dealerDebtTransactionDTO.ReferenceNo,
                    Note = dealerDebtTransactionDTO.Note,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _unitOfWork.DealerDebtTransactionRepository.AddAsync(transaction, ct);

                var period = await _unitOfWork.DealerDebtRepository.GetOrCreateQuarterAsync(dealerDebtTransactionDTO.DealerId, now, ct);

                switch (dealerDebtTransactionDTO.Type)
                {
                    case DealerDebtTransactionType.Purchase:
                        period.PurchasesAmount += dealerDebtTransactionDTO.Amount;
                        break;

                    case DealerDebtTransactionType.Payment:
                        period.PaymentsAmount += dealerDebtTransactionDTO.Amount;
                        break;

                    case DealerDebtTransactionType.Commission:
                        period.CommissionsAmount += dealerDebtTransactionDTO.Amount;
                        break;

                    case DealerDebtTransactionType.Penalty:
                        period.PenaltiesAmount += dealerDebtTransactionDTO.Amount;
                        break;

                    case DealerDebtTransactionType.Adjustment:
                        if (transaction.IsIncrease)
                            period.PurchasesAmount += dealerDebtTransactionDTO.Amount;
                        else
                            period.PaymentsAmount += dealerDebtTransactionDTO.Amount;
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported transaction type: {dealerDebtTransactionDTO.Type}");
                }

                RecalculatePeriod(period);

                _unitOfWork.DealerDebtRepository.Update(period);

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    StatusCode = 201,
                    IsSuccess = true,
                    Message = "Dealer debt transaction created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Error to create dealer debt transaction: {ex.Message}"
                };
            }
        }

        private static void RecalculatePeriod(DealerDebt debt)
        {
            var closing = debt.OpeningBalance + debt.PurchasesAmount + debt.PenaltiesAmount
                        - debt.PaymentsAmount - debt.CommissionsAmount;

            if (closing < 0)
            {
                debt.OverpaidAmount = -closing;
                debt.ClosingBalance = 0;
            }
            else
            {
                debt.OverpaidAmount = 0;
                debt.ClosingBalance = closing;
            }
        }

        public async Task<ResponseDTO<List<DealerDebtTransaction>>> GetAll(Guid dealerId, DateTime fromUtc, DateTime toUtc, int pageNumber, int pageSize, CancellationToken ct)
        {
            try
            {
                fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
                toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

                Expression<Func<DealerDebtTransaction, bool>> filter = dbt => dbt.DealerId == dealerId && dbt.OccurredAtUtc >= fromUtc
                      && dbt.OccurredAtUtc <= toUtc; ;

                (IReadOnlyList<DealerDebtTransaction> items, int total) result;
                result = await _unitOfWork.DealerDebtTransactionRepository.GetPagedAsync(
                            filter: filter,
                            includes: null,
                            orderBy: dm => dm.OccurredAtUtc,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var getDealerDebtTransactionList = _mapper.Map<List<GetDealerDebtTransactionDTO>>(result.items);
                return new ResponseDTO<List<DealerDebtTransaction>>
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Get dealer debt transactions successfully.",
                    Result = new
                    {
                        data = getDealerDebtTransactionList,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<List<DealerDebtTransaction>>
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Error to get dealer debt transactions: {ex.Message}"
                };
            }
        }
    }
}
