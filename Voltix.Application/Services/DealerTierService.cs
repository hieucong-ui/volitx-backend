using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dealer;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class DealerTierService : IDealerTierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private const decimal DEFAULT_COMMISSION = 0m;
        private const decimal DEFAULT_CREDIT = 0m;
        private const decimal DEFAULT_LATE_PENALTY = 0m;
        private const decimal DEFAULT_DEPOSIT = 0m;
        public DealerTierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllDealerTiers(CancellationToken ct)
        {
            try
            {
                var dealerTiers = (await _unitOfWork.DealerTierRepository.GetAllAsync()).OrderByDescending(dt => dt.Level);

                var getdealerTiers = _mapper.Map<List<GetDealerTierDTO>>(dealerTiers);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Dealer Tiers retrieved successfully.",
                    Result = getdealerTiers
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"An error occurred while retrieving Dealer Tiers: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateDealerTier(Guid dealerTierId, UpdateDealerTierDTO updateDealer, CancellationToken ct)
        {
            try
            {
                var dealerTier = await _unitOfWork.DealerTierRepository.GetByIdAsync(dealerTierId, ct);
                if (dealerTier is null)
                {
                    return new ResponseDTO
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Dealer Tier not found."
                    };
                }

                dealerTier.Name = updateDealer.Name ?? dealerTier.Name;
                dealerTier.Description = updateDealer.Description ?? dealerTier.Description;

                if (updateDealer.Level.HasValue)
                    dealerTier.Level = updateDealer.Level.Value;

                if (updateDealer.BaseCommissionPercent.HasValue)
                    dealerTier.BaseCommissionPercent = updateDealer.BaseCommissionPercent.Value;

                if (updateDealer.BaseDepositPercent.HasValue)
                    dealerTier.BaseDepositPercent = updateDealer.BaseDepositPercent.Value;

                if (updateDealer.BaseLatePenaltyPercent.HasValue)
                    dealerTier.BaseLatePenaltyPercent = updateDealer.BaseLatePenaltyPercent.Value;

                if (updateDealer.BaseCreditLimit.HasValue)
                    dealerTier.BaseCreditLimit = updateDealer.BaseCreditLimit.Value;

                dealerTier.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.DealerTierRepository.Update(dealerTier);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Dealer Tier updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"An error occurred while updating the Dealer Tier: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateDealerPolicyOverrideAsync(Guid dealerId, CreateDealerPolicyOverrideDTO createDealerPolicy, CancellationToken ct)
        {
            try
            {
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer not found"
                    };
                }

                var now = DateTime.UtcNow;
                var effFrom = createDealerPolicy.EffectiveFrom ?? now;
                DateTime? effTo = createDealerPolicy.EffectiveTo;

                if (effTo.HasValue && effTo.Value < effFrom)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "EffectiveTo must be greater than EffectiveFrom"
                    };
                }

                var activeOverrides = await _unitOfWork.DealerPolicyOverrideRepository.GetActiveByDealerAsync(dealerId, ct);
                if (activeOverrides is not null)
                {
                    activeOverrides.IsActive = false;
                    activeOverrides.UpdatedAt = now;
                    _unitOfWork.DealerPolicyOverrideRepository.Update(activeOverrides);
                }

                var entity = new DealerPolicyOverride
                {
                    Id = Guid.NewGuid(),
                    DealerId = dealerId,
                    CommissionPercent = createDealerPolicy.CommissionPercent,
                    CreditLimit = createDealerPolicy.CreditLimit,
                    LatePenaltyPercent = createDealerPolicy.LatePenaltyPercent,
                    DepositPercent = createDealerPolicy.DepositPercent,
                    EffectiveFrom = effFrom,
                    EffectiveTo = effTo,
                    IsActive = true,
                    Note = createDealerPolicy.Note,
                    CreatedAt = now,
                };

                await _unitOfWork.DealerPolicyOverrideRepository.AddAsync(entity, ct);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Dealer policy override created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create dealer policy override: {ex.Message}"
                };
            }
        }

        public async Task<DealerEffectivePolicyDTO> GetEffectivePolicyAsync(Guid dealerId, CancellationToken ct)
        {
            try
            {
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dealerId, ct);
                if (dealer is null)
                {
                    return new DealerEffectivePolicyDTO
                    {
                        DealerId = dealerId,
                        DealerName = "(not found)",
                        Source = "NotFound",
                        CommissionPercent = DEFAULT_COMMISSION,
                        CreditLimit = DEFAULT_CREDIT,
                        LatePenaltyPercent = DEFAULT_LATE_PENALTY,
                        DepositPercent = DEFAULT_DEPOSIT,
                    };
                }

                var now = DateTime.UtcNow;
                var activeOverride = await _unitOfWork.DealerPolicyOverrideRepository.GetCurrentActiveAsync(dealerId, now, ct);

                if (activeOverride is not null)
                {
                    return new DealerEffectivePolicyDTO
                    {
                        DealerId = dealer.Id,
                        DealerName = dealer.Name,
                        DealerTierId = dealer.DealerTierId,
                        DealerTierLevel = dealer.DealerTier?.Level,
                        DealerTierName = dealer.DealerTier?.Name,
                        Source = "Override",
                        CommissionPercent = activeOverride.CommissionPercent
                            ?? dealer.DealerTier?.BaseCommissionPercent
                            ?? DEFAULT_COMMISSION,
                        CreditLimit = activeOverride.CreditLimit
                            ?? dealer.DealerTier?.BaseCreditLimit
                            ?? DEFAULT_CREDIT,
                        LatePenaltyPercent = activeOverride.LatePenaltyPercent
                            ?? dealer.DealerTier?.BaseLatePenaltyPercent
                            ?? DEFAULT_LATE_PENALTY,
                        DepositPercent = activeOverride.DepositPercent
                            ?? dealer.DealerTier?.BaseDepositPercent
                            ?? DEFAULT_DEPOSIT,
                        OverrideId = activeOverride.Id,
                        OverrideNote = activeOverride.Note,
                        OverrideEffectiveFrom = activeOverride.EffectiveFrom,
                        OverrideEffectiveTo = activeOverride.EffectiveTo,
                        ResolvedAt = now
                    };
                }
                else if (dealer.DealerTier is not null)
                {
                    return new DealerEffectivePolicyDTO
                    {
                        DealerId = dealer.Id,
                        DealerName = dealer.Name,
                        DealerTierId = dealer.DealerTierId,
                        DealerTierLevel = dealer.DealerTier.Level,
                        DealerTierName = dealer.DealerTier.Name,
                        Source = "Tier",
                        CommissionPercent = dealer.DealerTier.BaseCommissionPercent ?? DEFAULT_COMMISSION,
                        CreditLimit = dealer.DealerTier.BaseCreditLimit ?? DEFAULT_CREDIT,
                        LatePenaltyPercent = dealer.DealerTier.BaseLatePenaltyPercent ?? DEFAULT_LATE_PENALTY,
                        DepositPercent = dealer.DealerTier.BaseDepositPercent ?? DEFAULT_DEPOSIT,
                        ResolvedAt = now
                    };
                }
                else
                {
                    return new DealerEffectivePolicyDTO
                    {
                        DealerId = dealer.Id,
                        DealerName = dealer.Name,
                        DealerTierId = null,
                        DealerTierLevel = null,
                        DealerTierName = null,
                        Source = "Default",
                        CommissionPercent = DEFAULT_COMMISSION,
                        CreditLimit = DEFAULT_CREDIT,
                        LatePenaltyPercent = DEFAULT_LATE_PENALTY,
                        DepositPercent = DEFAULT_DEPOSIT,
                        ResolvedAt = now
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving effective dealer policy : {ex.Message}");
            }
        }
    }
}
