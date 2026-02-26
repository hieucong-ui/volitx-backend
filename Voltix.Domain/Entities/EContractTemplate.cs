using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class EContractTemplate
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ContentHtml { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        private EContractTemplate() { }
        public EContractTemplate(string code, string name, string contentHtml)
        {
            Id = Guid.NewGuid();
            Code = code;
            Name = name;
            ContentHtml = contentHtml;
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdateContentHtml(string contentHtml)
        {
            ContentHtml = contentHtml;
        }

        public void Update(string name, string contentHtml)
        {
            Name = name;
            ContentHtml = contentHtml;
        }

    }
}
