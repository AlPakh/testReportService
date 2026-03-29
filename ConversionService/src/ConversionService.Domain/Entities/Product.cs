using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Entities
{
    public sealed class Product
    {
        private Product()
        {
        }

        public Product(string id, string name, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Product id must be specified.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name must be specified.", nameof(name));
            }

            Id = id;
            Name = name;
            IsActive = isActive;
        }

        public string Id { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        public bool IsActive { get; private set; }
    }
}