using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MoneyMate.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des catégories.
    /// Supporte :
    /// - Création/modification/suppression de catégories
    /// - Association multiple Budget ↔ Category
    /// - Calcul dynamique du montant prévu selon le pourcentage
    /// </summary>
    public partial class CategoryViewModel : BaseViewModel
    {
        // ============================================================================
        // 🔹 SERVICES
        // ============================================================================

        private readonly BudgetCategoryService _budgetCategoryService;


        // ============================================================================
        // 🔹 PROPRIÉTÉS OBSERVABLES
        // ============================================================================

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string? selectedColor;

        [ObservableProperty]
        private double percentage = 10;

        [ObservableProperty]
        private int editingCategoryId = 0;


        // ============================================================================
        // 🔹 COLLECTIONS
        // ============================================================================

        /// <summary>Liste globale des catégories existantes.</summary>
        public ObservableCollection<Category> Categories { get; } = new();

        /// <summary>Liste de tous les budgets disponibles.</summary>
        public ObservableCollection<Budget> Budgets { get; } = new();

        /// <summary>Budgets sélectionnés pour la catégorie (multi-association).</summary>
        public ObservableCollection<Budget> SelectedBudgets { get; } = new();

        /// <summary>Palette de couleurs disponibles pour une catégorie.</summary>
        public ObservableCollection<string> ColorOptions { get; } = new()
        {
            "#FFAFAD", "#FFD6A5", "#FCFEB6",
            "#CAFEBF", "#9DF3FD", "#A2C3FD",
            "#BFB3FD", "#FFC7FC"
        };


        // ============================================================================
        // 🔹 PROPRIÉTÉS CALCULÉES
        // ============================================================================

        /// <summary>
        /// Aperçu du montant moyen alloué (sur tous les budgets sélectionnés).
        /// </summary>
        public double AmountPreview
        {
            get
            {
                if (SelectedBudgets.Count == 0 || Percentage <= 0)
                    return 0;

                double total = 0;
                foreach (var b in SelectedBudgets)
                    total += b.TotalAmount * (Percentage / 100.0);

                return total / SelectedBudgets.Count;
            }
        }


        // ============================================================================
        // 🔹 CONSTRUCTEUR
        // ============================================================================

        public CategoryViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService,
            BudgetCategoryService budgetCategoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
            _budgetCategoryService = budgetCategoryService;

            _ = InitializeAsync();
        }


        // ============================================================================
        // 🔹 INITIALISATION
        // ============================================================================

        private async Task InitializeAsync()
        {
            await LoadCurrentUserAsync();
            await LoadBudgetsAsync();
            await LoadCategoriesAsync();
        }

        private async Task LoadBudgetsAsync()
        {
            await RunSafeAsync(async () =>
            {
                Budgets.Clear();

                // Charge tous les budgets (ou filtre par CurrentUserId si nécessaire)
                var list = await _budgetService.GetBudgetsAsync();

                foreach (var b in list.OrderByDescending(x => x.Year)
                                      .ThenByDescending(x => x.Month))
                {
                    Budgets.Add(b);
                }
            });
        }

        private async Task LoadCategoriesAsync()
        {
            await RunSafeAsync(async () =>
            {
                Categories.Clear();
                var list = await _categoryService.GetAllCategoriesAsync();

                foreach (var c in list.OrderBy(c => c.Name))
                    Categories.Add(c);
            });
        }


        // ============================================================================
        // 🔹 MULTI-SÉLECTION DES BUDGETS
        // ============================================================================

        /// <summary>
        /// Ajoute/retire un budget de la sélection (toggle).
        /// </summary>
        public void ToggleBudget(Budget budget)
        {
            if (SelectedBudgets.Contains(budget))
                SelectedBudgets.Remove(budget);
            else
                SelectedBudgets.Add(budget);

            OnPropertyChanged(nameof(AmountPreview));
        }


        // ============================================================================
        // 🔹 COMMANDES : AJOUT CATÉGORIE
        // ============================================================================

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            await RunSafeAsync(async () =>
            {
                // VALIDATIONS
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

                if (Percentage <= 0 || Percentage > 100)
                {
                    ShowMessage("Le pourcentage doit être entre 1 et 100%.", Colors.Red);
                    return;
                }

                // Récupère le budget actif (mois courant)
                var activeBudget = await GetActiveBudgetAsync();
                if (activeBudget == null)
                {
                    ShowMessage("Aucun budget actif trouvé. Créez d'abord un budget.", Colors.Red);
                    return;
                }

                // 1) Création de la catégorie globale
                var category = new Category
                {
                    Name = Name.Trim(),
                    ColorHex = SelectedColor!,
                    CreatedAt = DateTime.Now
                };

                await _categoryService.AddCategoryAsync(category);

                // 2) Création du lien BudgetCategory avec le budget actif
                var link = new BudgetCategory
                {
                    BudgetId = activeBudget.Id,
                    CategoryId = category.Id,
                    Percentage = Percentage,
                    SpentAmount = 0,
                    CreatedAt = DateTime.Now
                };

                await _budgetCategoryService.AddAsync(link, activeBudget.TotalAmount);

                // Mise à jour de la liste
                Categories.Add(category);

                await ShowSuccessMessage("Catégorie créée avec succès.");
                ResetForm();
            });
        }


        // ============================================================================
        // 🔹 CHARGEMENT D'UNE CATÉGORIE POUR ÉDITION
        // ============================================================================

        /// <summary>
        /// Charge une catégorie existante pour la modifier.
        /// </summary>
        public async Task LoadCategoryForEditAsync(int categoryId)
        {
            await RunSafeAsync(async () =>
            {
                editingCategoryId = categoryId;

                var category = await _categoryService.GetByIdAsync(categoryId);
                if (category == null)
                {
                    ShowMessage("Catégorie introuvable.", Colors.Red);
                    return;
                }

                Name = category.Name;
                SelectedColor = category.ColorHex;

                // Budgets liés à cette catégorie
                SelectedBudgets.Clear();
                var budgetsForCategory = await _budgetCategoryService.GetBudgetsForCategoryAsync(categoryId);

                foreach (var b in budgetsForCategory)
                    SelectedBudgets.Add(b);

                // Récupère un pourcentage "référence" (première association)
                var allLinks = await _budgetCategoryService.GetAllAsync();
                var firstLink = allLinks.FirstOrDefault(x => x.CategoryId == categoryId);

                Percentage = firstLink?.Percentage ?? 10;

                OnPropertyChanged(nameof(AmountPreview));
            });
        }


        // ============================================================================
        // 🔹 COMMANDES : MISE À JOUR CATÉGORIE
        // ============================================================================

        [RelayCommand]
        private async Task UpdateCategoryAsync()
        {
            if (editingCategoryId <= 0)
            {
                ShowMessage("Aucune catégorie sélectionnée pour la modification.", Colors.Red);
                return;
            }

            await RunSafeAsync(async () =>
            {
                // VALIDATIONS
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

                if (SelectedBudgets.Count == 0)
                {
                    ShowMessage("Veuillez sélectionner au moins un budget.", Colors.Red);
                    return;
                }

                if (Percentage <= 0 || Percentage > 100)
                {
                    ShowMessage("Le pourcentage doit être entre 1 et 100%.", Colors.Red);
                    return;
                }

                // 1) Mise à jour de la catégorie
                var category = await _categoryService.GetByIdAsync(editingCategoryId);
                if (category == null)
                {
                    ShowMessage("Catégorie introuvable.", Colors.Red);
                    return;
                }

                category.Name = Name.Trim();
                category.ColorHex = SelectedColor!;

                await _categoryService.UpdateCategoryAsync(category);

                // 2) Synchronisation des liens BudgetCategory
                await SyncBudgetLinksAsync(category);

                await ShowSuccessMessage("Catégorie mise à jour.");
                await LoadCategoriesAsync();
            });
        }

        /// <summary>
        /// Synchronise les liens BudgetCategory avec la sélection actuelle de budgets.
        /// </summary>
        private async Task SyncBudgetLinksAsync(Category category)
        {
            var allLinks = await _budgetCategoryService.GetAllAsync();
            var linksForCategory = allLinks.Where(x => x.CategoryId == category.Id).ToList();

            var selectedIds = SelectedBudgets.Select(b => b.Id).ToHashSet();
            var existingIds = linksForCategory.Select(l => l.BudgetId).ToHashSet();

            // Budgets à ajouter
            var toAdd = selectedIds.Except(existingIds).ToList();
            // Budgets à enlever
            var toRemove = existingIds.Except(selectedIds).ToList();
            // Budgets à mettre à jour (pourcentage / limite)
            var toUpdate = selectedIds.Intersect(existingIds).ToList();

            // Ajout
            foreach (var budgetId in toAdd)
            {
                var budget = Budgets.First(b => b.Id == budgetId);

                var newLink = new BudgetCategory
                {
                    BudgetId = budget.Id,
                    CategoryId = category.Id,
                    Percentage = Percentage,
                    SpentAmount = 0,
                    CreatedAt = DateTime.Now
                };

                await _budgetCategoryService.AddAsync(newLink, budget.TotalAmount);
            }

            // Suppression
            foreach (var budgetId in toRemove)
            {
                var link = linksForCategory.FirstOrDefault(l => l.BudgetId == budgetId);
                if (link != null)
                    await _budgetCategoryService.DeleteAsync(link);
            }

            // Mise à jour (pourcentage / limite)
            foreach (var budgetId in toUpdate)
            {
                var budget = Budgets.First(b => b.Id == budgetId);
                var link = linksForCategory.First(l => l.BudgetId == budgetId);

                link.Percentage = Percentage;
                await _budgetCategoryService.UpdateAsync(link, budget.TotalAmount);
            }
        }


        // ============================================================================
        // 🔹 COMMANDES : SUPPRESSION CATÉGORIE
        // ============================================================================

        [RelayCommand]
        private async Task DeleteCategoryAsync()
        {
            if (editingCategoryId <= 0)
            {
                ShowMessage("Aucune catégorie sélectionnée pour la suppression.", Colors.Red);
                return;
            }

            await RunSafeAsync(async () =>
            {
                bool confirm = await ShowConfirmAlert(
                    "Suppression",
                    "Voulez-vous vraiment supprimer cette catégorie ?"
                );

                if (!confirm) return;

                // Suppression de tous les liens
                await _budgetCategoryService.DeleteLinksForCategoryAsync(editingCategoryId);

                // Suppression de la catégorie
                await _categoryService.DeleteCategoryAsync(editingCategoryId);

                // Mise à jour de la liste locale
                var existing = Categories.FirstOrDefault(c => c.Id == editingCategoryId);
                if (existing != null)
                    Categories.Remove(existing);

                await ShowSuccessMessage("Catégorie supprimée.");
                ResetForm();
            });
        }


        // ============================================================================
        // 🔹 ANNULATION / RESET FORM
        // ============================================================================

        [RelayCommand]
        private void Cancel()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            Name = string.Empty;
            SelectedColor = null;
            Percentage = 10;

            SelectedBudgets.Clear();
            editingCategoryId = 0;

            Message = string.Empty;
            MessageColor = Colors.Transparent;

            OnPropertyChanged(nameof(AmountPreview));
        }


        // ============================================================================
        // 🔹 HOOKS GÉNÉRÉS PAR ObservableProperty
        // ============================================================================

        partial void OnPercentageChanged(double value)
        {
            OnPropertyChanged(nameof(AmountPreview));
        }

        // ============================================================================
        // 🔹 MÉTHODES UTILITAIRES
        // ============================================================================

        /// <summary>
        /// Récupère le budget actif (mois en cours) de l'utilisateur connecté.
        /// </summary>
        private async Task<Budget?> GetActiveBudgetAsync()
        {
            if (CurrentUserId <= 0)
                return null;

            var budgets = await _budgetService.GetBudgetsByUserAsync(CurrentUserId);
            var now = DateTime.Now;

            return budgets.FirstOrDefault(b =>
                b.Month == now.Month &&
                b.Year == now.Year &&
                b.IsActive);
        }
    }
}