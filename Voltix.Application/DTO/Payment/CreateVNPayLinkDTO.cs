using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Voltix.Application.DTO.Payment
{
    public class CreateVNPayLinkDTO
    {
        public string Locale { get; set; } = null!;
    }
}
