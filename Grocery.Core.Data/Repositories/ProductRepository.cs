using Grocery.Core.Data.Helpers;           // DatabaseConnection helper (Open/Close/Execute/CreateTable)
using Grocery.Core.Interfaces.Repositories; // IProductRepository interface
using Grocery.Core.Models;                 // Product model
using Microsoft.Data.Sqlite;               // SQLite ADO.NET provider
using System.Collections.Generic;

namespace Grocery.Core.Data.Repositories
{
    // Repository talks to SQLite table "Product"
    // Inherits DatabaseConnection so we can use OpenConnection/CloseConnection/CreateTable
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        // an option local cache if you want, not needed
        private readonly List<Product> products = new();

        public ProductRepository()
        {
            // Make sure table exists fields match your Product model
            //    Id, Name, Stock, ExpirationDate, Price
            CreateTable(@"
CREATE TABLE IF NOT EXISTS Product (
  Id              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  Name            NVARCHAR(100) NOT NULL UNIQUE,
  Stock           INTEGER NOT NULL CHECK(Stock >= 0),
  ExpirationDate  DATE NOT NULL,
  Price           REAL NOT NULL CHECK(Price >= 0)
)");

            // not needed but an option. Seed a few default rows like old in memory list
            //    INSERT OR IGNORE ensures we don't duplicate on app restarts
            List<string> seed = new()
            {
                @"INSERT OR IGNORE INTO Product(Name, Stock, ExpirationDate, Price)
                  VALUES('Melk', 300, '2025-09-25', 0.95)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ExpirationDate, Price)
                  VALUES('Kaas', 100, '2025-09-30', 7.98)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ExpirationDate, Price)
                  VALUES('Brood', 400, '2025-09-12', 2.19)",
                @"INSERT OR IGNORE INTO Product(Name, Stock, ExpirationDate, Price)
                  VALUES('Cornflakes', 0, '2025-12-31', 1.48)"
            };
            InsertMultipleWithTransaction(seed);
        }

        // Read All
        // Returns all products from the database
        public List<Product> GetAll()
        {
            products.Clear();

            // Select all fields store ExpirationDate as DATE
            string sql = "SELECT Id, Name, Stock, date(ExpirationDate), Price FROM Product ORDER BY Id";
            OpenConnection();
            using (var cmd = new SqliteCommand(sql, Connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    // SQLite returns DateTime for DATE; convert to DateOnly
                    DateOnly expiration = DateOnly.FromDateTime(reader.GetDateTime(3));
                    // SQLite REAL -> double, convert to decimal for your model
                    decimal price = (decimal)reader.GetDouble(4);

                    products.Add(new Product(id, name, stock, expiration, price));
                }
            }
            CloseConnection();

            return products;
        }

        // reads one
        // Returns a single product by Id or null if not found
        public Product? Get(int id)
        {
            Product? result = null;
            string sql = $"SELECT Id, Name, Stock, date(ExpirationDate), Price FROM Product WHERE Id = {id}";
            OpenConnection();
            using (var cmd = new SqliteCommand(sql, Connection))
            {
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int pid = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly expiration = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = (decimal)reader.GetDouble(4);

                    result = new Product(pid, name, stock, expiration, price);
                }
            }
            CloseConnection();
            return result;
        }

        // Create
        // Inserts a new product and returns it with the generated Id
        public Product Add(Product item)
        {
            // basic guard clauses
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new System.ArgumentException("Name is required");
            if (item.Stock < 0)
                throw new System.ArgumentException("Stock must be >= 0");
            if (item.Price < 0)
                throw new System.ArgumentException("Price must be >= 0");

            string insert = @"
INSERT INTO Product(Name, Stock, ExpirationDate, Price)
VALUES(@Name, @Stock, @ExpirationDate, @Price)
RETURNING RowId;";

            OpenConnection();
            using (var cmd = new SqliteCommand(insert, Connection))
            {
                // Add parameters safe against SQL injection
                cmd.Parameters.AddWithValue("@Name", item.Name.Trim());
                cmd.Parameters.AddWithValue("@Stock", item.Stock);
                // ExpirationDate pass as DateTime SQLite stores as TEXT/ NUMERIC internally
                cmd.Parameters.AddWithValue("@ExpirationDate", item.ShelfLife.ToDateTime(new System.TimeOnly(0, 0)));
                // Price SQLite REAL expects double
                cmd.Parameters.AddWithValue("@Price", (double)item.Price);

                // Get the new generated Id
                item.Id = System.Convert.ToInt32(cmd.ExecuteScalar());
            }
            CloseConnection();

            return item;
        }

        // Update
        // Updates Name, Stock, ExpirationDate, Price for an existing product
        public Product? Update(Product item)
        {
            string update = @"
UPDATE Product
   SET Name = @Name,
       Stock = @Stock,
       ExpirationDate = @ExpirationDate,
       Price = @Price
 WHERE Id = @Id;";

            OpenConnection();
            using (var cmd = new SqliteCommand(update, Connection))
            {
                cmd.Parameters.AddWithValue("@Name", item.Name.Trim());
                cmd.Parameters.AddWithValue("@Stock", item.Stock);
                cmd.Parameters.AddWithValue("@ExpirationDate", item.ShelfLife.ToDateTime(new System.TimeOnly(0, 0)));
                cmd.Parameters.AddWithValue("@Price", (double)item.Price);
                cmd.Parameters.AddWithValue("@Id", item.Id);

                cmd.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }

        // Delete
        // Deletes the product by Id and returns the deleted item (or null if nothing)
        public Product? Delete(Product item)
        {
            string delete = $"DELETE FROM Product WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(delete);
            CloseConnection();
            return item;
        }
    }
}
