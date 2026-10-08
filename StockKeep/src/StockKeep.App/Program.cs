using StockKeep.Core;

namespace StockKeep.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // The database lives in the user's local app-data folder, e.g.
        // C:\Users\<you>\AppData\Local\StockKeep\stockkeep.db
        var dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StockKeep");
        Directory.CreateDirectory(dataFolder);

        var repository = SqliteProductRepository.ForFile(Path.Combine(dataFolder, "stockkeep.db"));
        var service = new ProductService(repository);

        Application.Run(new MainForm(service));
    }
}
