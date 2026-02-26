using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptResult
    {
        public bool Success { get; set; }
        public ResultCode Code { get; set; }
        public List<string> Messages { get; set; } = [];
        public object? Result { get; set; }
        public VnptResult()
        {
        }

        public VnptResult(bool success)
        {
            Success = success;
        }

        public VnptResult(params string[] messages)
        {
            Messages = messages.ToList();
        }
    }

    public class VnptResult<TResultData> : VnptResult
    {
        public TResultData? Data { get; set; }
        public VnptResult()
        {
        }

        public VnptResult(bool success)
        {
            Success = success;
        }

        public VnptResult(TResultData data)
        {
            Success = true;
            Data = data;
        }

        public VnptResult(params string[] messages)
        {
            Messages = messages.ToList();
        }
    }
}
