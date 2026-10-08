# Database setup

The application uses a local SQL Server database named `ShelfMarket`.

## First run

1. Install and start the SQL Server Database Engine on the computer. SSMS is not required to run the application.
2. Make sure the current Windows user can create a database on that SQL Server instance.
3. Clone or pull the project, open the solution in Visual Studio, and run the application. On startup it creates `ShelfMarket`, creates or updates the tables, and inserts the 80 standard shelves if the shelf table is empty.

The setup is safe to run again: it does not drop existing tables or rows. Each computer has its own local database; data is not shared through GitHub.

## Named SQL Server instance

The default instance name is `localhost`. If SQL Server was installed as a named instance, set the `REOLMARKET_SQL_SERVER` environment variable before starting Visual Studio. For example, use `localhost\SQLEXPRESS` for an instance named `SQLEXPRESS`.

The SQL initialization script is embedded in the application, so SSMS is not needed for normal first-time setup.
