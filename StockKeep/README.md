# StockKeep

[![CI](https://github.com/estherpolie/StockKeep/actions/workflows/ci.yml/badge.svg)](https://github.com/estherpolie/StockKeep/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A small, simple desktop inventory manager for Windows, built with C# WinForms and SQLite.

Add, view, edit, search and delete products. Items that are running low are highlighted so you know what to reorder.

## Features

- **CRUD** for products: name, SKU, quantity, price and a low-stock threshold
- **Search** by name or SKU as you type
- **Low-stock highlighting** and a count in the status bar
- **Validation**: required fields, no negative numbers, unique SKUs (case-insensitive)
- **Local storage** in a single SQLite file, with no server to install

## Getting started

**Requirements:** Windows 10/11 and the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(or Visual Studio 2022 with the *.NET desktop development* workload).

```bash
git clone https://github.com/estherpolie/StockKeep.git
cd StockKeep
dotnet run --project src/StockKeep.App
```

Or open `StockKeep.sln` in Visual Studio and press **F5**.

Data is stored at `%LOCALAPPDATA%\StockKeep\stockkeep.db`. Delete that file to start fresh.

## Running the tests

```bash
dotnet test
```

The tests cover the `StockKeep.Core` library (validation, business rules and the SQLite repository) using an in-memory database. They run on any OS.

## Project structure

```
src/
  StockKeep.Core/     Business logic and storage. No UI code; fully unit-tested.
    Product.cs                  The data model
    ProductValidator.cs         Field validation rules
    ProductService.cs           Business rules used by the UI (validation + unique SKU)
    IProductRepository.cs       Storage interface
    SqliteProductRepository.cs  SQLite implementation (parameterised queries only)
  StockKeep.App/      WinForms UI. Thin layer that calls ProductService.
tests/
  StockKeep.Core.Tests/  xUnit tests
```

The UI never talks to the database directly. All rules live in `StockKeep.Core`, so they are tested and can't be bypassed by the forms.

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and our [Code of Conduct](CODE_OF_CONDUCT.md) first.
Good first issues are labelled [`good first issue`](https://github.com/estherpolie/StockKeep/labels/good%20first%20issue).

## License

[MIT](LICENSE) © Esther Poli
