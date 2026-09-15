using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using StockManager.Core;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Page « Stock » : indicateurs, recherche et tableau des produits.
/// </summary>
public class StockViewModel : ObservableObject
{
    private readonly StockService _stockService;
    private readonly IDialogService _dialogService;

    private List<ProductViewModel> _allProducts = new();
    private string _searchText = string.Empty;
    private int _productCount;
    private int _totalQuantity;
    private int _lowStockCount;
    private int _lowStockThreshold = StockService.DefaultLowStockThreshold;

    public StockViewModel(StockService stockService, IDialogService dialogService)
    {
        _stockService = stockService;
        _dialogService = dialogService;

        AddProductCommand = new RelayCommand(AddProduct);
        IncrementCommand = new RelayCommand(parameter => Adjust(parameter, product => product.Increment()));
        DecrementCommand = new RelayCommand(parameter => Adjust(parameter, product => product.Decrement()));
        ConfirmChangeCommand = new RelayCommand(parameter => ConfirmChange(parameter as ProductViewModel));
        CancelChangeCommand = new RelayCommand(parameter => Adjust(parameter, product => product.CancelPending()));
        BeginEditCommand = new RelayCommand(parameter => Adjust(parameter, product => product.BeginEdit()));
        CommitEditCommand = new RelayCommand(parameter => Adjust(parameter, product => product.CommitEdit()));
        EditProductCommand = new RelayCommand(parameter => EditProduct(parameter as ProductViewModel));
        DeleteProductCommand = new RelayCommand(parameter => DeleteProduct(parameter as ProductViewModel));
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
    }

    /// <summary>Produits affichés, filtrés par la recherche.</summary>
    public ObservableCollection<ProductViewModel> Products { get; } = new();

    public ICommand AddProductCommand { get; }

    public ICommand IncrementCommand { get; }

    public ICommand DecrementCommand { get; }

    public ICommand ConfirmChangeCommand { get; }

    public ICommand CancelChangeCommand { get; }

    public ICommand BeginEditCommand { get; }

    public ICommand CommitEditCommand { get; }

    public ICommand EditProductCommand { get; }

    public ICommand DeleteProductCommand { get; }

    public ICommand ClearSearchCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
                OnPropertyChanged(nameof(HasSearchText));
            }
        }
    }

    public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

    /// <summary>Nombre de produits de l'inventaire (indépendant de la recherche).</summary>
    public int ProductCount
    {
        get => _productCount;
        private set => SetProperty(ref _productCount, value);
    }

    /// <summary>Total des articles en stock.</summary>
    public int TotalQuantity
    {
        get => _totalQuantity;
        private set => SetProperty(ref _totalQuantity, value);
    }

    /// <summary>Nombre de produits dont le stock est faible ou épuisé.</summary>
    public int LowStockCount
    {
        get => _lowStockCount;
        private set => SetProperty(ref _lowStockCount, value);
    }

    /// <summary>Seuil courant, affiché sous l'indicateur de stock faible.</summary>
    public int LowStockThreshold
    {
        get => _lowStockThreshold;
        private set
        {
            if (SetProperty(ref _lowStockThreshold, value))
            {
                OnPropertyChanged(nameof(LowStockCaption));
            }
        }
    }

    public string LowStockCaption => $"Seuil : {LowStockThreshold} ou moins";

    /// <summary>L'inventaire contient au moins un produit.</summary>
    public bool HasProducts => _allProducts.Count > 0;

    /// <summary>La recherche en cours ne renvoie aucun produit.</summary>
    public bool HasNoSearchResults => HasProducts && Products.Count == 0;

    /// <summary>L'inventaire est complètement vide.</summary>
    public bool IsInventoryEmpty => !HasProducts;

    /// <summary>Recharge l'inventaire depuis la base de données.</summary>
    public void Refresh()
    {
        LowStockThreshold = _stockService.GetLowStockThreshold();

        var products = _stockService.GetProducts();

        _allProducts = products
            .Select(product => new ProductViewModel(product, LowStockThreshold))
            .ToList();

        UpdateSummary();

        ApplyFilter();
        OnPropertyChanged(nameof(HasProducts));
    }

    /// <summary>Recalcule les trois indicateurs à partir de l'inventaire chargé.</summary>
    private void UpdateSummary()
    {
        ProductCount = _allProducts.Count;
        TotalQuantity = _allProducts.Sum(product => product.Quantity);
        LowStockCount = _allProducts.Count(product => product.NeedsAttention);
    }

    private void ApplyFilter()
    {
        var matches = _allProducts.Where(Matches).ToList();

        Products.Clear();

        foreach (var product in matches)
        {
            Products.Add(product);
        }

        OnPropertyChanged(nameof(HasNoSearchResults));
        OnPropertyChanged(nameof(IsInventoryEmpty));
    }

    private bool Matches(ProductViewModel product)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return Normalize(product.Name).Contains(Normalize(SearchText), StringComparison.Ordinal);
    }

    /// <summary>
    /// Met le texte en minuscules et retire les accents pour que la recherche
    /// fonctionne quelle que soit la façon de taper (« taies » trouve « Taies »).
    /// </summary>
    private static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLower(CultureInfo.CurrentCulture).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    // ------------------------------------------------------------------ actions

    private void AddProduct()
    {
        if (_dialogService.ShowAddProduct())
        {
            Refresh();
        }
    }

    /// <summary>Applique un ajustement à une ligne, sans rien enregistrer.</summary>
    private static void Adjust(object? parameter, Action<ProductViewModel> action)
    {
        if (parameter is ProductViewModel product)
        {
            action(product);
        }
    }

    /// <summary>
    /// Enregistre l'ajustement en attente d'une ligne : un seul mouvement est
    /// créé pour l'écart total (une entrée s'il est positif, une sortie sinon).
    /// Les autres lignes conservent leur ajustement en cours.
    /// </summary>
    private void ConfirmChange(ProductViewModel? product)
    {
        if (product is null || !product.HasPendingChange)
        {
            return;
        }

        var delta = product.Delta;

        try
        {
            var saved = delta > 0
                ? _stockService.AddStock(product.Id, delta)
                : _stockService.RemoveStock(product.Id, -delta);

            product.ApplySavedQuantity(saved.Quantity);
            UpdateSummary();
        }
        catch (StockException exception)
        {
            _dialogService.ShowError(exception.Message);
            Refresh();
        }
    }

    private void EditProduct(ProductViewModel? product)
    {
        if (product is not null && _dialogService.ShowEditProduct(product.Model))
        {
            Refresh();
        }
    }

    private void DeleteProduct(ProductViewModel? product)
    {
        if (product is null || !_dialogService.ConfirmDeleteProduct(product.Name))
        {
            return;
        }

        try
        {
            _stockService.DeleteProduct(product.Id);
        }
        catch (StockException exception)
        {
            _dialogService.ShowError(exception.Message);
        }

        Refresh();
    }
}
