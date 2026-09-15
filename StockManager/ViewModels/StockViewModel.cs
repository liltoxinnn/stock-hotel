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
        AddStockCommand = new RelayCommand(parameter => AddStock(parameter as ProductViewModel));
        RemoveStockCommand = new RelayCommand(parameter => RemoveStock(parameter as ProductViewModel));
        EditProductCommand = new RelayCommand(parameter => EditProduct(parameter as ProductViewModel));
        DeleteProductCommand = new RelayCommand(parameter => DeleteProduct(parameter as ProductViewModel));
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
    }

    /// <summary>Produits affichés, filtrés par la recherche.</summary>
    public ObservableCollection<ProductViewModel> Products { get; } = new();

    public ICommand AddProductCommand { get; }

    public ICommand AddStockCommand { get; }

    public ICommand RemoveStockCommand { get; }

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

        ProductCount = _allProducts.Count;
        TotalQuantity = _allProducts.Sum(product => product.Quantity);
        LowStockCount = _allProducts.Count(product => product.NeedsAttention);

        ApplyFilter();
        OnPropertyChanged(nameof(HasProducts));
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

    private void AddStock(ProductViewModel? product)
    {
        if (product is not null && _dialogService.ShowAddStock(product.Model))
        {
            Refresh();
        }
    }

    private void RemoveStock(ProductViewModel? product)
    {
        if (product is not null && _dialogService.ShowRemoveStock(product.Model))
        {
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
