using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.S3;
using Voltix.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IS3Service
    {
        ResponseDTO GenerateUploadUrl(string objectKey, string contentType);
        string GenerateDownloadUrl(string objectKey);
        ResponseDTO GenerateUploadElectricVehicle(PreSignedUploadDTO preSignedUploadDTO);
        ResponseDTO GenerateUploadDealerFBAttachment(PreSignedUploadDTO preSignedUploadDTO);
        ResponseDTO GenerateUploadCustomerFBAttachment(PreSignedUploadDTO preSignedUploadDTO);
        Task RemoveElectricVehicleFile(string key);
    }
}
