using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Log;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Voltix.Application.Services
{
    public class LogService : ILogService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public LogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> AddLogAsync(ClaimsPrincipal user, LogType logType, string entityName, string description, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404,

                    };
                }

                var fullName = user.FindFirst("FullName")?.Value ?? "Unknown User";

                Guid? dealerId = null;
                if (role == StaticUserRole.DealerManager)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404

                        };
                    }
                    dealerId = dealer.Id;
                }
                var log = new Log
                {
                    LogType = logType,
                    Description = $"{fullName} {ConvertLogTypeToString(logType)} {entityName} {description}",
                    UserId = userId,
                    DealerId = dealerId,
                    EntityName = entityName,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.LogRepository.AddAsync(log, ct);
                await _unitOfWork.SaveAsync(ct);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Log added successfully",
                    StatusCode = 201,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 5000
                };
            }
        }

        public async Task<ResponseDTO> GetAllLogsAsync(ClaimsPrincipal user, int pageNumber, int pageSize, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404,
                    };
                }
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Log> log = _unitOfWork.LogRepository.Query();

                if (role == StaticUserRole.Admin)
                {
                    log = log.Where(l => l.DealerId == null);
                }
                else if (role == StaticUserRole.DealerManager)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                    log = log.Where(l => l.DealerId == dealer.Id);
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var logs = await log
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new GetLogDTO
                    {
                        DealerId = l.DealerId,
                        UserId = l.UserId,
                        FullName = l.User.FullName,
                        Description = l.Description,
                        EntityName = l.EntityName,
                        LogType = l.LogType,
                        CreatedAt = l.CreatedAt
                    })
                    .ToListAsync(ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Logs retrieved successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        TotalItems = await log.CountAsync(ct),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Data = logs
                    }
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 5000
                };
            }
        }

        private string ConvertLogTypeToString(LogType logType)
        {
            return logType switch
            {
                LogType.Create => "created",
                LogType.Update => "updated",
                LogType.Delete => "deleted",
                _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
            };

        }
    }
}
