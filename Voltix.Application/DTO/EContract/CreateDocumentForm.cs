using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EContract
{
    public class CreateDocumentForm
    {
        [FromForm(Name = "no")] public string No { get; set; } = default!;
        [FromForm(Name = "subject")] public string Subject { get; set; } = default!;
        [FromForm(Name = "typeId")] public int TypeId { get; set; }
        [FromForm(Name = "departmentId")] public int DepartmentId { get; set; }
        [FromForm(Name = "description")] public string? Description { get; set; }
        [FromForm(Name = "file")] public IFormFile File { get; set; } = default!;
    }
}
