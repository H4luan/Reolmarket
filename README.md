# Reolmarked

WPF-applikation udviklet i C# til administration af et reolmarked. Programmet bruger MVVM og gemmer data i Microsoft SQL Server.

## Funktioner

- Administrér lejere, reoler og lejeaftaler
- Administrér produkter og lager
- Registrér salg og returneringer
- Beregn og gem månedsafregninger

## Krav

- Windows og .NET 10
- Visual Studio med WPF-understøttelse
- Microsoft SQL Server Database Engine

## Database

Programmet bruger databasen `ShelfMarket`. Ved opstart klargør `DatabaseInitializer` databasen ved hjælp af `Database/InitializeShelfMarket.sql`. Hvis reoltabelen er tom, oprettes der 80 standardreoler. Der indsættes ikke testlejere, produkter eller salg; de oprettes i programmet.

Programmet bruger Windows-integreret login til SQL Server. Standardserveren er `localhost`. Hvis du bruger en navngiven instans, kan serveren angives med miljøvariablen `REOLMARKET_SQL_SERVER`, for eksempel `localhost\SQLEXPRESS`.

## Start programmet

1. Sørg for, at SQL Server kører.
2. Åbn `Reolmarket.slnx` i Visual Studio.
3. Gendan NuGet-pakker.
4. Vælg `Reolmarket` som startprojekt, og tryk **F5**.

Databasen ligger lokalt på hver computer. Data deles ikke via GitHub.

## Repository

[github.com/H4luan/Reolmarket](https://github.com/H4luan/Reolmarket)