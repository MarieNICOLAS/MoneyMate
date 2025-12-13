using MoneyMate.ViewModels;

namespace MoneyMate.Views.ExpensesViews
{
    [QueryProperty(nameof(ExpenseId), "expenseId")]
    public partial class EditExpensePage : ContentPage
    {
        public int ExpenseId { get; set; }

        private readonly ExpenseViewModel _vm;

        public EditExpensePage(ExpenseViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Injecter l’ID dans le ViewModel
            _vm.ExpenseId = ExpenseId;

            // Charger la dépense depuis la DB
            await _vm.LoadExpenseAsync();
        }
    }
}
