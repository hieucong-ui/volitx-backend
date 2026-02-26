using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.S3;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Application.Services
{
    public class S3Service : IS3Service
    {
        private readonly string _bucketName;
        private readonly string? _accessKey;
        private readonly string? _secretKey;
        private readonly RegionEndpoint _region;

        public S3Service(IConfiguration config)
        {
            _bucketName = config["S3Bucket:bucketName"] ?? throw new ArgumentNullException("S3Bucket:bucketName");
            _accessKey = config["S3Settings:AccessKey"];
            _secretKey = config["S3Settings:SecretKey"];
            _region = RegionEndpoint.APSoutheast1;
        }

        private AmazonS3Client CreateS3Client()
        {
            if (!string.IsNullOrEmpty(_accessKey) && !string.IsNullOrEmpty(_secretKey))
                return new AmazonS3Client(_accessKey, _secretKey, _region);

            return new AmazonS3Client(_region);
        }

        public string GenerateDownloadUrl(string objectKey)
        {
            var s3Client = CreateS3Client();

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            var url = s3Client.GetPreSignedURL(request);

            return url;
        }

        public ResponseDTO GenerateUploadUrl(string objectKey, string contentType)
        {
            var s3Client = CreateS3Client();

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(5),
                ContentType = contentType
            };

            var url = s3Client.GetPreSignedURL(request);
            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Generate upload URL successfully",
                StatusCode = 200,
                Result = new
                {
                    UploadUrl = url,
                    ObjectKey = objectKey
                }
            };
        }

        public async Task RemoveFile(string objectKey)
        {
            var s3Client = CreateS3Client();
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
            };
            await s3Client.DeleteObjectAsync(deleteObjectRequest);
        }

        public async Task RemoveElectricVehicleFile(string key)
        {
            var objectKey = $"{StaticBucketName.ElectricVehicleBucket}/{key}";
            await RemoveFile(objectKey);
        }

        // Generate Upload Url for ElectricVehicle
        public ResponseDTO GenerateUploadElectricVehicle(PreSignedUploadDTO preSignedUploadDTO)
        {
            var objectKey = $"{StaticBucketName.ElectricVehicleBucket}/{Guid.NewGuid()}_{preSignedUploadDTO.FileName}";
            return GenerateUploadUrl(objectKey, preSignedUploadDTO.ContentType);
        }

        // Generate Upload Url for Dealer Feedback Attachment
        public ResponseDTO GenerateUploadDealerFBAttachment(PreSignedUploadDTO preSignedUploadDTO)
        {
            var objectKey = $"{StaticBucketName.DealerFeedbackBucket}/{Guid.NewGuid()}_{preSignedUploadDTO.FileName}";
            return GenerateUploadUrl(objectKey, preSignedUploadDTO.ContentType);
        }

        // Generate Upload Url for Customer Feedback Attachment
        public ResponseDTO GenerateUploadCustomerFBAttachment(PreSignedUploadDTO preSignedUploadDTO)
        {
            var objectKey = $"{StaticBucketName.CustomerFeedbackBucket}/{Guid.NewGuid()}_{preSignedUploadDTO.FileName}";
            return GenerateUploadUrl(objectKey, preSignedUploadDTO.ContentType);
        }
    }
}
