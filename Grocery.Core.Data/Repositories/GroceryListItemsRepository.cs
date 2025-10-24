using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        // local cache temporary storage in memory
        private readonly List<GroceryListItem> groceryListItems = new();

        // Constructor runs when new GroceryListItemsRepository() is created
        public GroceryListItemsRepository()
        {
            // Create the GroceryListItem table if it does not exist already
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL)");

            // Immediately load all items from database into the list
            GetAll();
        }

        // Returns ALL GroceryListItem rows from the database
        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear(); // clear local list so we don’t duplicate
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";
            OpenConnection(); // open database connection

            using (SqliteCommand command = new(selectQuery, Connection)) // prepare SQL command
            {
                SqliteDataReader reader = command.ExecuteReader(); // execute and read result

                while (reader.Read()) // loop through each row
                {
                    int id = reader.GetInt32(0);  // read first column id
                    int groceryListId = reader.GetInt32(1); // second column
                    int productId = reader.GetInt32(2); // third column
                    int amount = reader.GetInt32(3); // fourth column

                    // Add this row as a new GroceryListItem object into the list
                    groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, amount));
                }
            }

            CloseConnection(); // close database connection
            return groceryListItems; // return the list
        }

        // Get all items for ONE specific grocery list
        // returns only the items that belong to one grocery list
        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> result = new List<GroceryListItem>();
            
            // Loop over ALL items, check if the GroceryListId matches
            foreach (var item in GetAll())
            {
                if (item.GroceryListId == groceryListId)
                {
                    result.Add(item); // add matching item into result list
                }
            }

            return result; // return filtered list
        }

        // Add a new item into the database
        // just like insert a new GroceryListItem into the database
        public GroceryListItem Add(GroceryListItem item)
        {
            string insertQuery = @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) 
                                   VALUES(@GroceryListId, @ProductId, @Amount) Returning RowId;";

            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                // pass values safely with parameters
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);

                // Execute and get back the new row id
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();

            return item; // return the item with its new id
        }

        // Delete one GroceryListItem from the database
        public GroceryListItem? Delete(GroceryListItem item)
        {
            string deleteQuery = $"DELETE FROM GroceryListItem WHERE Id = {item.Id};";

            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery); // execute delete
            CloseConnection();

            return item; // return the deleted item
        }

        // Get one GroceryListItem by its id
        public GroceryListItem? Get(int id)
        {
            GroceryListItem? listItem = null; // start with nothing
            string selectQuery = $"SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = {id}";

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read()) // if we found a row
                {
                    int Id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);

                    listItem = new GroceryListItem(Id, groceryListId, productId, amount);
                }
            }

            CloseConnection();
            return listItem; // return the found item or null if not found
        }

        // Update an existing GroceryListItem in the database change amount
        public GroceryListItem? Update(GroceryListItem item)
        {
            string updateQuery = $"UPDATE GroceryListItem SET Amount = @Amount WHERE Id = {item.Id};";

            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("Amount", item.Amount);
                command.ExecuteNonQuery(); // run update
            }
            CloseConnection();

            return item;  // return updated item
        }
    }
}
