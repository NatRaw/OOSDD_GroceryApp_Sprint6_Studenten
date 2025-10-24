using Grocery.App.ViewModels;

namespace Grocery.App.Views;

public partial class ProductView : ContentPage
{
    private readonly ProductViewModel _vm;

    public ProductView(ProductViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    // this runs every time the page is shown again
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshProducts(); // reload products from Database
    }
}