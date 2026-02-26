using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Auth
{
    public class ResponseDTO
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
        public object? Result { get; set; }

        public ResponseDTO()
        {
        }

        public ResponseDTO(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public ResponseDTO(string message)
        {
            Message = message;
        }
    }

    public class ResponseDTO<TResultData> : ResponseDTO
    {
        public new TResultData? Data { get; set; }
        public ResponseDTO()
        {
        }

        public ResponseDTO(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public ResponseDTO(TResultData data)
        {
            IsSuccess = true;
            Data = data;
        }

        public ResponseDTO(string message)
        {
            Message = message;
        }
    }
}
