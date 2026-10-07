using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure
{
    public class ShelfRepository
    {
        public List<Shelf> GetAll()

        {
            var shelves = new List<Shelf>();

            using SqlConnection connection =
                DatabaseConnection.CreateConnection();

            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText =

                """
                SELECT ShelfID, Number, Location, NumberOfShelves, NumberOfClothingRails
                FROM Shelf
                ORDER BY Number;
                """;

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int numberOfShelves = reader.GetInt32(3);
                int numberOfClothingRails = reader.GetInt32(4);

                shelves.Add(new Shelf
                {
                    ShelfId = reader.GetInt32(0),
                    ShelfNumber = reader.IsDBNull(1)
                    ? 0
                    : reader.GetInt32(1),
                    Location = reader.IsDBNull(2)
                    ? string.Empty
                    : reader.GetString(2),
                    Layout = GetLayout(numberOfShelves, numberOfClothingRails) });
            }

            return shelves;

        }

        public void Add(Shelf shelf)

        {

            (int numberOfShelves, int numberOfClothingRails) =
                GetLayoutValues(shelf.Layout);

            using SqlConnection connection =
                DatabaseConnection.CreateConnection();
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO Shelf (Number, Location, NumberOfShelves, NumberOfClothingRails)
                OUTPUT INSERTED.ShelfID
                VALUES
                (@Number, @Location, @NumberOfShelves, @NumberOfClothingRails);
                """;

            command.Parameters.AddWithValue("@Number", shelf.ShelfNumber);
            command.Parameters.AddWithValue("@Location", shelf.Location);
            command.Parameters.AddWithValue("@NumberOfShelves", numberOfShelves);
            command.Parameters.AddWithValue("@NumberOfClothingRails", numberOfClothingRails);

            shelf.ShelfId = (int)command.ExecuteScalar()!;

        }


        public void Update(Shelf shelf)
        {

            (int numberOfShelves, int numberOfClothingRails) =
                GetLayoutValues(shelf.Layout);

                using SqlConnection connection =
                DatabaseConnection.CreateConnection();

            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText =
                """
                UPDATE Shelf
                SET Number = @Number,
                Location = @Location,
                NumberOfShelves  = @NumberOfShelves,
                NumberOfClothingRails = @NumberOfClothingRails
                WHERE ShelfID = @ShelfID;
                """;

            command.Parameters.AddWithValue("@ShelfID", shelf.ShelfId);
            command.Parameters.AddWithValue("@Number", shelf.ShelfNumber);
            command.Parameters.AddWithValue("@Location", shelf.Location);
            command.Parameters.AddWithValue("@NumberOfShelves", numberOfShelves);
            command.Parameters.AddWithValue(
                "@NumberOfClothingRails",
                numberOfClothingRails);

            int rowsUpdated = command.ExecuteNonQuery();

            if (rowsUpdated == 0)
            {
                throw new InvalidOperationException(
                 $"Reol med ID {shelf.ShelfId} blev ikke fundet.");

            }

        }

        public void Delete(int shelfId)

        {
            using SqlConnection connection = DatabaseConnection.CreateConnection();

            connection.Open();

            using SqlCommand command = connection.CreateCommand();


            command.CommandText =
            """
            DELETE FROM Shelf
            WHERE ShelfID = @ShelfID;
            """;

            command.Parameters.AddWithValue("@ShelfID", shelfId);

            int rowsDeleted = command.ExecuteNonQuery();

            if (rowsDeleted == 0)
            {
                throw new InvalidOperationException(
                $"Reol med ID {shelfId} blev ikke fundet.");

            }

        }

        private static (int Shelves, int Rails) GetLayoutValues(
            ShelfLayout layout)
        {
            return layout switch
            {
                ShelfLayout.SixShelves => (6, 0),
                ShelfLayout.ThreeShelvesAndRail => (3, 1),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(layout),
                    layout,
                    "Ukendt reolindretning.")
            };
        }

        private static ShelfLayout GetLayout(int shelves, int rails)
        {
            if (shelves == 6 && rails == 0)
            {
                return ShelfLayout.SixShelves;
            }

            if (shelves == 3 && rails == 1)
            {
                return ShelfLayout.ThreeShelvesAndRail;
            }

            throw new InvalidOperationException($"Ukendt reolindretning: {shelves} hylder og {rails} bøjlestænger");
        }



    } }
