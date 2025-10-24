using Grocery.Core.Models;

namespace Grocery.Core.Interfaces.Services
{
    public interface IProductService
    {
        public List<Product> GetAll();
        
        //With role parameter
        Product Add(Product item, string role = "User");
        
        public Product? Delete(Product item);

        public Product? Get(int id);

        public Product? Update(Product item);
    }
}
