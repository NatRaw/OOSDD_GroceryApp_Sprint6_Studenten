using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;

namespace Grocery.App.ViewModels
{
    // ViewModel for product list screen
    public partial class ProductViewModel : ObservableObject
    {
        private readonly IProductService _productService;

        // list with all products
        public ObservableCollection<Product> Products { get; set; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;
            // load all products from db
            Products = new ObservableCollection<Product>(_productService.GetAll());
        }

        // command for toolbar button -> go to NewProductView
        [RelayCommand]
        private async Task AddNewProduct()
        {
            await Shell.Current.GoToAsync(nameof(NewProductView));
        }

        // reload products after adding new one
        public void RefreshProducts()
        {
            Products.Clear();
            foreach (var p in _productService.GetAll())
                Products.Add(p);
        }
    }
}