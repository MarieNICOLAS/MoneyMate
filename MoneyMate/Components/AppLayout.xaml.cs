using Microsoft.Maui.Controls;

namespace MoneyMate.Components;

public partial class AppLayout : ContentView
{
	public AppLayout()
	{
		InitializeComponent();
	}
    public static readonly BindableProperty PageContentProperty =
                BindableProperty.Create(nameof(Content), typeof(View), typeof(AppLayout), propertyChanged: OnContentChanged);

    public View Content
    {
        get => (View)GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var layout = (AppLayout)bindable;
        layout.PageContent.Content = (View)newValue;
    }
}