using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContractTemplate;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class EContractTemplateService : IEContractTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public EContractTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> CreateEContractTemplateAsync(CreateEContractTemplateDTO templateDTO, CancellationToken ct)
        {
            try
            {
                var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateDTO.Code, ct);
                if (template is not null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template already exists",
                        StatusCode = 404
                    };
                }

                EContractTemplate newTemplate = new EContractTemplate(templateDTO.Code, templateDTO.Name, templateDTO.Html);
                await _unitOfWork.EContractTemplateRepository.AddAsync(newTemplate, ct);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template created successfully",
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred at CreateContractTemplateAsync in ContractTemplateService: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetEContractTemplateByIdAsync(Guid eContractTemplateId, CancellationToken ct)
        {
            try
            {
                var template = await _unitOfWork.EContractTemplateRepository.GetbyIdAsync(eContractTemplateId, ct);
                if (template is null || template.IsDeleted)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template not found",
                        StatusCode = 404
                    };
                }

                var getTemplateDTO = _mapper.Map<GetEContractTemplateDTO>(template);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template retrieved successfully",
                    StatusCode = 200,
                    Result = getTemplateDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred at GetEContractTemplateByEcontractIdAsync in EContractTemplateService: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAll(int? pageNumber, int? pageSize, CancellationToken ct)
        {
            try
            {
                var templates = (await _unitOfWork.EContractTemplateRepository.GetAllAsync()).Where(t => t.IsDeleted == false);

                if (pageNumber > 0 && pageSize > 0)
                {
                    templates = templates.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize).ToList();
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid page number or page size",
                        StatusCode = 400
                    };
                }

                var templateList = _mapper.Map<List<GetEContractTemplateDTO>>(templates);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Templates retrieved successfully",
                    StatusCode = 200,
                    Result = templateList
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred at GetAll in EContractTemplateService: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateEcontractTemplateAsync(string code, UpdateEContractTemplateDTO templateDTO, CancellationToken ct)
        {
            try
            {
                var existingTemplate = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(code, ct);
                if (existingTemplate is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template not found",
                        StatusCode = 404
                    };
                }

                if (existingTemplate.IsDeleted)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update a deleted template",
                        StatusCode = 400
                    };
                }

                if (templateDTO.Name is not null && templateDTO.Html is null)
                {
                    existingTemplate.UpdateName(templateDTO.Name);
                }
                else if (templateDTO.Html is not null && templateDTO.Name is null)
                {
                    existingTemplate.UpdateContentHtml(templateDTO.Html);
                }
                else if (templateDTO.Name is not null && templateDTO.Html is not null)
                {
                    existingTemplate.Update(templateDTO.Name, templateDTO.Html);
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No fields to update",
                        StatusCode = 401
                    };
                }

                _unitOfWork.EContractTemplateRepository.Update(existingTemplate);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template updated successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred at UpdateEcontractTemplateAsync in EContractTemplateService: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
