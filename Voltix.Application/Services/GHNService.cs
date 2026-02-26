using Voltix.Application.DTO.Auth;
using Voltix.Application.IServices;
using Voltix.Infrastructure.IClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class GHNService : IGHNService
    {
        private readonly IGHNClient _ghnClient;
        public GHNService(IGHNClient ghnClient)
        {
            _ghnClient = ghnClient;
        }

        public async Task<ResponseDTO> GetDistrictsAsync(int provinceId)
        {
            try
            {
                var response = await _ghnClient.GetDistrictsAsync(provinceId);

                if (response.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Success to get district list",
                        Result = response.Data
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = response.Message ?? "Failed to get district list",
                        StatusCode = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetProvincesAsync()
        {
            try
            {
                var response = await _ghnClient.GetProvincesAsync();

                if (response.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Success to get province list",
                        Result = response.Data
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = response.Message ?? "Failed to get province list",
                        StatusCode = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ProvincesOpenGetProvinceResponse(CancellationToken ct)
        {
            try
            {
                var response = await _ghnClient.ProvincesOpenGetProvinceResponse(ct);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Success to get province list",
                    Result = response
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ProvincesOpenGetWardResponse(string provinceCode, CancellationToken ct)
        {
            try
            {
                var response = await _ghnClient.ProvincesOpenGetWardResponse(provinceCode, ct);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Success to get ward list",
                    Result = response
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }
        
        public async Task<ResponseDTO> GetWardsAsync(int districtId)
        {
            try
            {
                var response = await _ghnClient.GetWardsAsync(districtId);

                if (response.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Success to get wards list",
                        Result = response.Data
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = response.Message ?? "Failed to get wards list",
                        StatusCode = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
