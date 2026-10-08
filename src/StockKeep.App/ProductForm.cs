using StockKeep.Core;

namespace StockKeep.App;

/// <summary>
/// Dialog for adding a new product or editing an existing one.
/// Saving goes through <see cref="ProductService"/>, so all validation rules apply.
/// </summary>
public class ProductForm : Form
{
    private readonly ProductService _service;
    private readonly Product? _existing;

    private readonly TextBox _nameBox = new() { Dock = DockStyle.Fill, MaxLength = ProductValidator.MaxNameLength };
    private readonly TextBox _skuBox = new() { Dock = DockStyle.Fill, MaxLength = ProductValidator.MaxSkuLength, CharacterCasing = CharacterCasing.Upper };
    private readonly NumericUpDown _quantityBox = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1_000_000 };
    private readonly NumericUpDown _priceBox = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10_000_000, DecimalPlaces = 2, ThousandsSeparator = true };
    private readonly NumericUpDown _thresholdBox = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1_000_000 };

    /// <param name="product">The product to edit, or null to add a new one.</param>
    public ProductForm(ProductService service, Product? product)
    {
        _service = service;
        _existing = product;

        Text = product is null ? "Add product" : $"Edit {product.Sku}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        BuildLayout();

        if (product is not null)
        {
            _nameBox.Text = product.Name;
            _skuBox.Text = product.Sku;
            _quantityBox.Value = product.Quantity;
            _priceBox.Value = product.Price;
            _thresholdBox.Value = product.LowStockThreshold;
        }
        else
        {
            _thresholdBox.Value = new Product().LowStockThreshold;
        }
    }

    private void BuildLayout()
    {
        var table = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Padding = new Padding(10), Dock = DockStyle.Fill };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));

        AddRow(table, "&Name", _nameBox);
        AddRow(table, "&SKU", _skuBox);
        AddRow(table, "&Quantity", _quantityBox);
        AddRow(table, "&Price", _priceBox);
        AddRow(table, "&Low-stock at", _thresholdBox);

        var saveButton = new Button { Text = "Save", AutoSize = true };
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        saveButton.Click += (_, _) => Save();

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange([cancelButton, saveButton]);
        table.Controls.Add(buttons, 0, table.RowCount);
        table.SetColumnSpan(buttons, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Controls.Add(table);
    }

    private static void AddRow(TableLayoutPanel table, string label, Control input)
    {
        var row = table.RowCount;
        table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 10, 3) }, 0, row);
        table.Controls.Add(input, 1, row);
        table.RowCount = row + 1;
    }

    private void Save()
    {
        var product = new Product
        {
            Id = _existing?.Id ?? 0,
            Name = _nameBox.Text,
            Sku = _skuBox.Text,
            Quantity = (int)_quantityBox.Value,
            Price = _priceBox.Value,
            LowStockThreshold = (int)_thresholdBox.Value,
        };

        try
        {
            if (_existing is null)
                _service.Add(product);
            else
                _service.Update(product);

            DialogResult = DialogResult.OK; // closes the dialog
        }
        catch (ValidationException ex)
        {
            // Keep the dialog open so the user can fix the input.
            MessageBox.Show(this, string.Join(Environment.NewLine, ex.Errors), "Please fix the following",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save the product.\n\n{ex.Message}", "StockKeep",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
