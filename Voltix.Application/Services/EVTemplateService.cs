using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.ElectricVehicle;
using Voltix.Application.DTO.EVTemplate;
using Voltix.Application.DTO.Promotion;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Voltix.Application.Services
{
    public class EVTemplateService : IEVTemplateService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IS3Service _s3Service;

        public EVTemplateService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(_s3Service));
        }
        public async Task<ResponseDTO> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO)
        {
            try
            {
                ElectricVehicleTemplate template = new ElectricVehicleTemplate
                {
                    VersionId = createEVTemplateDTO.VersionId,
                    ColorId = createEVTemplateDTO.ColorId,
                    Description = createEVTemplateDTO.Description,
                    Price = createEVTemplateDTO.Price,
                };
                if(template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template is null.",
                        StatusCode = 404
                    };
                }

                foreach (var key in createEVTemplateDTO.AttachmentKeys)
                {
                    var fileName = Path.GetFileName(key);
                    template.EVAttachments.Add(new EVAttachment
                    {
                        FileName = fileName,
                        Key = key
                    });
                }

                await _unitOfWork.EVTemplateRepository.AddAsync(template,CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template created successfully",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> DeleteEVTemplateAsync(Guid EVTemplateId)
        {
            try
            {
                var templates = await _unitOfWork.EVTemplateRepository.GetByIdAsync(EVTemplateId);
                if(templates == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template not found",
                        StatusCode = 404
                    };
                }

                templates.IsActive = false; //soft delete
                _unitOfWork.EVTemplateRepository.Update(templates);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Delete promotion successfully",
                    StatusCode = 200,
                };

            }catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllVehicleTemplateAsync(int pageNumber, int pageSize, string? search, Guid? templateId, decimal?　minPrice, decimal? maxPrice, bool sortByPriceAsc ,CancellationToken ct)
        {
            try
            {
                Func<IQueryable<ElectricVehicleTemplate>, IQueryable<ElectricVehicleTemplate>> includes = q => q
                    .Include(t => t.Version)
                        .ThenInclude(v => v.Model)
                    .Include(t => t.Color);

                Expression<Func<ElectricVehicleTemplate, bool>> filter = t =>
                    t.IsActive == true &&
                    (!templateId.HasValue || t.Id == templateId.Value) &&
                    (!minPrice.HasValue || t.Price >= minPrice.Value) &&
                    (!maxPrice.HasValue || t.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var lowered = search.Trim().ToLower();
                    filter = t =>
                        t.IsActive == true &&
                        (!templateId.HasValue || t.Id == templateId.Value) &&
                        (!minPrice.HasValue || t.Price >= minPrice.Value) &&
                        (!maxPrice.HasValue || t.Price <= maxPrice.Value) &&
                        (
                            (t.Version != null && t.Version.VersionName.ToLower().Contains(lowered)) ||
                            (t.Version.Model != null && t.Version.Model.ModelName.ToLower().Contains(lowered)) ||
                            (t.Color != null && t.Color.ColorName.ToLower().Contains(lowered))
                        );
                }

                (IReadOnlyList<ElectricVehicleTemplate> items, int total) result =
                    await _unitOfWork.EVTemplateRepository.GetPagedAsync(
                        filter: filter,
                        includes: includes,
                        orderBy: t => t.Price,
                        ascending: sortByPriceAsc,
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        ct: ct
                    );

                if (result.items == null || !result.items.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No vehicle templates found."
                    };
                }

                var getTemples = _mapper.Map<List<GetEVTemplateDTO>>(result.items);

                foreach (var tem in getTemples)
                {
                    var attachments = _unitOfWork.EVAttachmentRepository
                        .GetAttachmentsByElectricVehicleTemplateId(tem.Id);

                    var urlList = new List<string>();
                    foreach (var att in attachments)
                    {
                        var url = _s3Service.GenerateDownloadUrl(att.Key);
                        urlList.Add(url);
                    }

                    tem.ImgUrl = urlList;
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all Template successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        data = getTemples,
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
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetTemplatesByVersionAndColorAsync(Guid versionId, Guid colorId)
        {
            try
            {
                var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(versionId);
                if (version == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Version not found",
                        StatusCode = 404,
                    };
                }

                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(colorId);
                if(color == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Color not found",
                        StatusCode = 404,
                    };
                }
                

                var templates = await _unitOfWork.EVTemplateRepository.GetTemplatesByVersionAndColorAsync(versionId, colorId);
                if ( templates == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Template not found",
                        StatusCode = 404,
                    };
                }

                var getTemplates = _mapper.Map<GetEVTemplateDTO>(templates);

                var attachments = _unitOfWork.EVAttachmentRepository.GetAttachmentsByElectricVehicleTemplateId(templates.Id);

                var urlLists = new List<string>();
                foreach (var att in attachments)
                {
                    var url = _s3Service.GenerateDownloadUrl(att.Key);
                    urlLists.Add(url);
                }
                getTemplates.ImgUrl = urlLists;


                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get templates successfully",
                    StatusCode = 200,
                    Result = getTemplates
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleTemplateByIdAsync(Guid EVTemplateId)
        {
            try
            {
                var template = await _unitOfWork.EVTemplateRepository.Query(t => t.Id == EVTemplateId)
                    .Include(t => t.Version)
                        .ThenInclude(v => v.Model)
                    .Include(t => t.Color)
                    .Include(t => t.EVAttachments)
                    .FirstOrDefaultAsync();
                if (template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template not exist",
                        StatusCode = 404
                    };
                }

                var getTemplate = _mapper.Map<GetEVTemplateDTO>(template);
                if (template.EVAttachments != null && template.EVAttachments.Any())
                {
                    getTemplate.ImgUrl = template.EVAttachments
                        .Select(a => _s3Service.GenerateDownloadUrl(a.Key))
                        .ToList();
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = " Get Template successfully",
                    StatusCode = 200,
                    Result = getTemplate
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateEVTemplateAsync(Guid EVTemplateId,UpdateEVTemplateDTO updateEVTemplateDTO)
        {
            try
            {
                var template = await _unitOfWork.EVTemplateRepository.GetByIdAsync(EVTemplateId);
                if(template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Template not found",
                        StatusCode = 404
                    };
                }

                if (!string.IsNullOrWhiteSpace(updateEVTemplateDTO.Description))
                {
                    template.Description = updateEVTemplateDTO.Description.Trim();
                }
                if (updateEVTemplateDTO.Price.HasValue && updateEVTemplateDTO.Price.Value >= 0)
                    template.Price = updateEVTemplateDTO.Price.Value;

                //Take photo
                if (updateEVTemplateDTO.AttachmentKeys != null && updateEVTemplateDTO.AttachmentKeys.Any())
                {
                    var oldAttachments = template.EVAttachments.ToList();
                    try
                    {
                        // delete old photo
                        foreach (var att in oldAttachments)
                        {
                            await _s3Service.RemoveElectricVehicleFile(att.Key);
                            _unitOfWork.EVAttachmentRepository.Remove(att);
                        }

                        // remove from entity
                        template.EVAttachments.Clear();

                        // Add new photo
                        foreach (var key in updateEVTemplateDTO.AttachmentKeys)
                        {
                            var fileName = Path.GetFileName(key);
                            template.EVAttachments.Add(new EVAttachment
                            {
                                ElectricVehicleTemplateId = template.Id,
                                FileName = fileName,
                                Key = key
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // if delete photo error then rollback
                        foreach (var oldAtt in oldAttachments)
                            template.EVAttachments.Add(oldAtt);

                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = $"Failed to update attachments: {ex.Message}"
                        };
                    }
                }

                _unitOfWork.EVTemplateRepository.Update(template);
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
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
