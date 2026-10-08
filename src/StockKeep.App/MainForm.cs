using StockKeep.Core;

namespace StockKeep.App;

/// <summary>
/// Main window: a searchable product list with Add / Edit / Delete buttons.
/// The layout is built in code (no designer file) so the whole UI can be read in one place.
/// </summary>
public class MainForm : Form
{
    private readonly ProductService _service;

    private readonly TextBox _searchBox = new() { PlaceholderText = "Search by name or SKU…", Dock = DockStyle.Fill };
    private readonly Button _addButton = new() { Text = "&Add", AutoSize = true };
    private readonly Button _editButton = new() { Text = "&Edit", AutoSize = true };
    private readonly Button _deleteButton = new() { Text = "&Delete", AutoSize = true };
    private readonly DataGridView _grid = new();
    private readonly ToolStripStatusLabel _statusLabel = new();

    public MainForm(ProductService service)
    {
        _service = service;

        Text = "StockKeep";
        MinimumSize = new Size(700, 450);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        ConfigureGrid();

        _searchBox.TextChanged += (_, _) => LoadProducts();
        _addButton.Click += (_, _) => AddProduct();
        _editButton.Click += (_, _) => EditSelected();
        _deleteButton.Click += (_, _) => DeleteSelected();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };
        _grid.SelectionChanged += (_, _) => UpdateButtons();

        LoadProducts();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        toolbar.Controls.AddRange([_addButton, _editButton, _deleteButton]);

        var top = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(6) };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.Controls.Add(_searchBox, 0, 0);
        top.Controls.Add(toolbar, 1, 0);

        var status = new StatusStrip();
        status.Items.Add(_statusLabel);

        _grid.Dock = DockStyle.Fill;

        // Order matters for docking: Fill control is added first so it takes the remaining space.
        Controls.Add(_grid);
        Controls.Add(top);
        Controls.Add(status);
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Product.Sku), HeaderText = "SKU", FillWeight = 20 },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Product.Name), HeaderText = "Name", FillWeight = 45 },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Product.Quantity), HeaderText = "Qty", FillWeight = 10 },
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Product.Price), HeaderText = "Price", FillWeight = 15,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
            },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Product.LowStockThreshold), HeaderText = "Low at", FillWeight = 10 });

        // Highlight low-stock rows.
        _grid.RowPrePaint += (_, e) =>
        {
            var row = _grid.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = row.DataBoundItem is Product { IsLowStock: true }
                ? Color.MistyRose
                : Color.Empty;
        };
    }

    private Product? SelectedProduct =>
        _grid.CurrentRow?.DataBoundItem as Product;

    private void LoadProducts()
    {
        try
        {
            var products = _service.List(_searchBox.Text);
            _grid.DataSource = products.ToList();
            var lowStock = _service.CountLowStock(products);
            _statusLabel.Text = $"{products.Count} product(s)" + (lowStock > 0 ? $"  •  {lowStock} low on stock" : "");
        }
        catch (Exception ex)
        {
            ShowError("Could not load products.", ex);
        }
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var hasSelection = SelectedProduct is not null;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private void AddProduct()
    {
        using var dialog = new ProductForm(_service, product: null);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadProducts();
    }

    private void EditSelected()
    {
        if (SelectedProduct is not { } selected)
            return;

        // Re-read from the database so we edit the latest saved values.
        var product = _service.Get(selected.Id);
        if (product is null)
        {
            MessageBox.Show(this, "This product no longer exists.", "StockKeep", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadProducts();
            return;
        }

        using var dialog = new ProductForm(_service, product);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadProducts();
    }

    private void DeleteSelected()
    {
        if (SelectedProduct is not { } selected)
            return;

        var answer = MessageBox.Show(this,
            $"Delete '{selected.Name}' ({selected.Sku})? This cannot be undone.",
            "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            _service.Delete(selected.Id);
        }
        catch (Exception ex)
        {
            ShowError("Could not delete the product.", ex);
        }
        LoadProducts();
    }

    private void ShowError(string message, Exception ex) =>
        MessageBox.Show(this, $"{message}\n\n{ex.Message}", "StockKeep", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
