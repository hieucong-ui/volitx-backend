using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EContract
{
    public class UpdateEContractDTO
    {
        public string Id { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string HtmlFile { get; set; } = null!;
    }
}
