namespace MoneyMate.Views;

public partial class BasePage : ContentPage
{
    public BasePage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Définit ou récupère le contenu injecté dynamiquement (ContentPresenter)
    /// </summary>
    public View PageContent
    {
        get => (View)DynamicPresenter.Content;
        set
        {
            DynamicPresenter.Content = value;
            if (value != null)
                value.BindingContext = BindingContext;
        }
    }
}
