using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;
using System.Threading.Tasks;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : ObservableObject
    {
        private readonly IProductService _productService;

        // Properties for the input fields bind to XAML
        [ObservableProperty] private string name;
        [ObservableProperty] private decimal price;
        [ObservableProperty] private int stock;
        [ObservableProperty] private DateTime shelfLife = DateTime.Today; // start with today

        // constructor gets ProductService so we can use it
        public NewProductViewModel(IProductService productService)
        {
            _productService = productService;
        }

        // command for the Save button
        [RelayCommand]
        private async Task SaveProduct()
        {
            try
            {
                // make a new Product object with the form values
                var newProduct = new Product(
                    id: 0,
                    name: Name,
                    stock: Stock,
                    shelfLife: DateOnly.FromDateTime(ShelfLife),
                    price: Price
                );

                // only Admin can add products
                _productService.Add(newProduct, "Admin");

                // show success message
                await Shell.Current.DisplayAlert("Succes", "Product toegevoegd!", "OK");

                // go back to previous page ProductView
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                // show error if something goes wrong
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}