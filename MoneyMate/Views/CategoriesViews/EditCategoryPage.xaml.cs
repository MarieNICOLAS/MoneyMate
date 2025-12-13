using MoneyMate.ViewModels;

namespace MoneyMate.Views.CategoriesViews;

[QueryProperty(nameof(CategoryId), "categoryId")]
public partial class EditCategoryPage : ContentPage
{
    private readonly CategoryViewModel _viewModel;

    public int CategoryId { get; set; }

    public EditCategoryPage(CategoryViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearAsync();

        // Charge la catégorie à éditer si un ID est passé
        if (CategoryId > 0)
        {
            await _viewModel.LoadCategoryForEditAsync(CategoryId);
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.OnDisappearAsync();
    }
}