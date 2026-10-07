CREATE DATABASE ShelfMarket;
GO

USE ShelfMarket;
GO

CREATE TABLE ShelfRenter
(
	ShelfRenterID INT PRIMARY KEY IDENTITY(1,1),
	Name NVARCHAR(100) NOT NULL,
	Phone NVARCHAR(20),
	Email NVARCHAR(100)
);

CREATE TABLE Shelf
(
	ShelfID INT PRIMARY KEY IDENTITY(1,1),
	Number INT,
	ShelfStatus NVARCHAR(20),
	Location NVARCHAR(100),
	NumberOfShelves INT,
	NumberOfClothingRails INT
);

CREATE TABLE RentalAgreement
(
	RentalAgreementID INT PRIMARY KEY IDENTITY(1,1),
	StartDate DATE NOT NULL,
	EndDate DATE NULL,
	Price DECIMAL(10,2) NOT NULL,
	ShelfRenterID INT NOT NULL,
	ShelfID INT NOT NULL,
	PaymentMethod NVARCHAR(20) NOT NULL CONSTRAINT DF_RentalAgreement_PaymentMethod DEFAULT ('Cash'),

	CONSTRAINT FK_RentalAgreement_ShelfRenter
		FOREIGN KEY (ShelfRenterID)
		REFERENCES ShelfRenter(ShelfRenterID),

	CONSTRAINT FK_RentalAgreement_Shelf
		FOREIGN KEY (ShelfID)
		REFERENCES Shelf(ShelfID)
);

CREATE TABLE Product
(
	ProductID INT PRIMARY KEY IDENTITY (1,1),
	Name NVARCHAR(100) NOT NULL,
	Price DECIMAL(10,2) NOT NULL,
	Barcode NVARCHAR(100) NOT NULL,
	StockQuantity INT NOT NULL,
	ShelfID INT NOT NULL,

	CONSTRAINT FK_Product_Shelf
		FOREIGN KEY (ShelfID)
		REFERENCES Shelf(ShelfID)
);

CREATE TABLE Employee
(
	EmployeeID INT PRIMARY KEY IDENTITY (1,1),
	Name NVARCHAR(100) NOT NULL,
	Phone NVARCHAR(100) NOT NULL,
	Email NVARCHAR(100) NOT NULL
);

CREATE TABLE Sale
(
	SaleID INT PRIMARY KEY IDENTITY (1,1),
	SaleDate DATE NOT NULL,
	TotalAmount DECIMAL(10,2) NOT NULL,
	EmployeeID INT NOT NULL,
	PaymentMethod NVARCHAR(20) NOT NULL CONSTRAINT DF_Sale_PaymentMethod DEFAULT ('Cash'),

	CONSTRAINT FK_Sale_Employee
		FOREIGN KEY (EmployeeID)
		REFERENCES Employee(EmployeeID)
);

CREATE TABLE SaleLine
(
	SaleLineID INT PRIMARY KEY IDENTITY (1,1),
	Quantity INT NOT NULL,
	SalePrice DECIMAL(10,2) NOT NULL,
	ProductID INT NOT NULL,
	SaleID INT NOT NULL,
	ShelfID INT NOT NULL,
	Comment NVARCHAR(100) NULL,

	CONSTRAINT FK_SaleLine_Product
		FOREIGN KEY (ProductID)
		REFERENCES Product(ProductID),

	CONSTRAINT FK_SaleLine_Shelf FOREIGN KEY (ShelfID) REFERENCES Shelf(ShelfID),
	CONSTRAINT FK_SaleLine_Sale
		FOREIGN KEY (SaleID)
		REFERENCES Sale(SaleID)
);

CREATE TABLE ProductReturn
(
	ProductReturnID INT PRIMARY KEY IDENTITY (1,1),
	ReturnDate DATE NOT NULL,
	Quantity INT NOT NULL,
	Amount DECIMAL(10,2) NOT NULL,
	Comment NVARCHAR(100),
	SaleLineID INT NOT NULL,

	CONSTRAINT FK_ProductReturn_SaleLine
		FOREIGN KEY (SaleLineID)
		REFERENCES SaleLine(SaleLineID)
);

CREATE TABLE Settlement
(
	SettlementID INT PRIMARY KEY IDENTITY (1,1),
	SettlementDate DATE NOT NULL,
	TotalSales DECIMAL(10,2) NOT NULL,
	
	TotalReturns DECIMAL(10,2) NOT NULL,
	Commission DECIMAL(10,2) NOT NULL,
	TotalRent DECIMAL(10,2) NOT NULL,
	Result DECIMAL(10,2) NOT NULL,
	ShelfRenterID INT NOT NULL,

	CONSTRAINT FK_Settlement_ShelfRenter
		FOREIGN KEY (ShelfRenterID)
		REFERENCES ShelfRenter(ShelfRenterID)
);
