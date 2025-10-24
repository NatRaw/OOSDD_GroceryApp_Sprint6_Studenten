using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;

namespace Grocery.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Get all products
        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        // Add new product only Admin
        public Product Add(Product item, string role = "User")
        {
            // simple role check
            if (role != "Admin")
                throw new UnauthorizedAccessException("Alleen admins mogen producten toevoegen.");

            // basic validation extra safety
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Naam is verplicht.");
            if (item.Price < 0)
                throw new ArgumentException("Prijs moet 0 of hoger zijn.");

            // save to db
            return _productRepository.Add(item);
        }

        // Get one product by id
        public Product? Get(int id)
        {
            return _productRepository.Get(id);
        }

        // Update passthrough
        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }

        // Delete passthrough
        public Product? Delete(Product item)
        {
            return _productRepository.Delete(item);
        }
    }
}