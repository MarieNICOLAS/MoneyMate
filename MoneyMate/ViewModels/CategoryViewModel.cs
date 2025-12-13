using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace MoneyMate.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;

        // -------------------------
        //        PROPRIÉTÉS
        // -------------------------

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string? _selectedColor;
        public string? SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        // Message UX
        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private Color _messageColor = Colors.Transparent;
        public Color MessageColor
        {
            get => _messageColor;
            set => SetProperty(ref _messageColor, value);
        }

        // Liste des catégories
        public ObservableCollection<Category> Categories { get; } = new();

        // Palette de couleurs
        public ObservableCollection<string> ColorOptions { get; } = new()
        {
            "#FFAFAD", "#FFD6A5", "#FCFEB6",
            "#CAFEBF", "#9DF3FD", "#A2C3FD",
            "#BFB3FD", "#FFC7FC"
        };

        // Catégorie actuellement en modification
        public int EditingCategoryId { get; set; }

        // -------------------------
        //        COMMANDES
        // -------------------------

        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand UpdateCategoryCommand { get; }
        public IAsyncRelayCommand DeleteCategoryCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // -------------------------
        //        CONSTRUCTEUR
        // -------------------------

        public CategoryViewModel()
        {
            _categoryService = new CategoryService(App.Database);

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            UpdateCategoryCommand = new AsyncRelayCommand(UpdateCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
            CancelCommand = new RelayCommand(Cancel);

            _ = LoadCategoriesAsync();
        }

        // -------------------------
        //        CHARGEMENT
        // -------------------------

        private async Task LoadCategoriesAsync()
        {
            var list = await _categoryService.GetCategoriesAsync();

            Categories.Clear();
            foreach (var c in list)
                Categories.Add(c);
        }

        // -------------------------
        //        AJOUT
        // -------------------------

        private async Task AddCategoryAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ShowMessage("Veuillez saisir un nom.", Colors.Red);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedColor))
            {
                ShowMessage("Veuillez choisir une couleur.", Colors.Red);
                return;
            }

            try
            {
                IsBusy = true;

                var category = new Category
                {
                    Name = Name.Trim(),
                    ColorHex = SelectedColor!,
                    CreatedAt = DateTime.Now
                };

                await _categoryService.AddCategoryAsync(category);
                Categories.Add(category);

                ResetForm();
                ShowMessage("Catégorie créée avec succès.", Colors.Green);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Colors.Red);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        //        MODIFICATION
        // -------------------------

        private async Task UpdateCategoryAsync()
        {
            if (IsBusy) return;

            var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
            if (category == null)
            {
                ShowMessage("Catégorie introuvable.", Colors.Red);
                return;
            }

            try
            {
                IsBusy = true;

                category.Name = Name.Trim();
                category.ColorHex = SelectedColor ?? "#CCCCCC";

                await _categoryService.UpdateCategoryAsync(category);

                ShowMessage("Catégorie mise à jour.", Colors.Green);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Colors.Red);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        //        SUPPRESSION
        // -------------------------

        private async Task DeleteCategoryAsync()
        {
            if (IsBusy) return;

            var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
            if (category == null)
            {
                ShowMessage("Catégorie introuvable.", Colors.Red);
                return;
            }

            await _categoryService.DeleteCategoryAsync(category);
            Categories.Remove(category);

            ShowMessage("Catégorie supprimée.", Colors.Green);
        }

        // -------------------------
        //        ANNULATION
        // -------------------------

        private void Cancel()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            Name = string.Empty;
            SelectedColor = null;
        }

        // -------------------------
        //        MESSAGE UX
        // -------------------------

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }
    }
}
