using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace COSCPFWA
{
    public partial class Store : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                return;
            }

            string itemType;
            if (!TryNormalizeItemType(Request.Form["item"], out itemType))
            {
                ShowAlert("Please select a valid store item.");
                return;
            }

            int quantitySold;
            if (!int.TryParse(Request.Form["quantity"], out quantitySold) || quantitySold <= 0)
            {
                ShowAlert("Quantity must be a positive whole number.");
                return;
            }

            SaveStoreSale(itemType, quantitySold);
        }

        private void SaveStoreSale(string itemType, int quantitySold)
        {
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;
            if (string.IsNullOrEmpty(connString))
            {
                ShowAlert("Database connection string is missing or misconfigured.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            EnsureInventoryAvailable(conn, transaction, itemType, quantitySold);

                            string query = @"
                                INSERT INTO store
                                    (transaction_type, customer_id, item_sold, quantity_sold, transaction_date, item_type)
                                VALUES
                                    (@TransactionType, @CustomerID, @ItemSold, @QuantitySold, @TransactionDate, @ItemType)";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                            {
                                int? customerId = GetCurrentCustomerId(conn, transaction);
                                cmd.Parameters.AddWithValue("@TransactionType", "Supply Sale");
                                cmd.Parameters.AddWithValue("@CustomerID", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@ItemSold", GetDisplayItemName(itemType));
                                cmd.Parameters.AddWithValue("@QuantitySold", quantitySold);
                                cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@ItemType", itemType);

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    ShowAlert("Store purchase saved successfully.");
                }
                catch (InvalidOperationException ex)
                {
                    ShowAlert(ex.Message);
                }
                catch (MySqlException ex)
                {
                    ShowAlert("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAlert("An unexpected error occurred: " + ex.Message);
                }
            }
        }

        private void EnsureInventoryAvailable(MySqlConnection conn, MySqlTransaction transaction, string itemType, int quantitySold)
        {
            string query = @"
                SELECT quantity_on_hand
                FROM inventory
                WHERE item_name = @ItemType
                LIMIT 1
                FOR UPDATE";

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ItemType", itemType);
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("This item is not configured in inventory. Please add it before selling it.");
                }

                int quantityOnHand = Convert.ToInt32(result);
                if (quantitySold > quantityOnHand)
                {
                    throw new InvalidOperationException("Not enough inventory is available for that sale.");
                }
            }
        }

        private int? GetCurrentCustomerId(MySqlConnection conn, MySqlTransaction transaction)
        {
            int customerId;
            if (Session["CustomerID"] != null && int.TryParse(Convert.ToString(Session["CustomerID"]), out customerId))
            {
                return customerId;
            }

            int userId;
            if (Session["UserID"] == null || !int.TryParse(Convert.ToString(Session["UserID"]), out userId))
            {
                return null;
            }

            string query = @"
                SELECT customer_id
                FROM customer
                WHERE user_id = @UserID
                LIMIT 1";

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                customerId = Convert.ToInt32(result);
                Session["CustomerID"] = customerId;
                return customerId;
            }
        }

        private bool TryNormalizeItemType(string submittedItem, out string itemType)
        {
            itemType = null;

            if (string.IsNullOrWhiteSpace(submittedItem))
            {
                return false;
            }

            switch (submittedItem.Trim().ToLowerInvariant())
            {
                case "paper":
                    itemType = "paper";
                    return true;
                case "bubble wrap":
                case "bubblewrap":
                    itemType = "bubble wrap";
                    return true;
                case "box":
                    itemType = "box";
                    return true;
                case "envelop":
                case "envelope":
                    itemType = "envelop";
                    return true;
                case "tape":
                    itemType = "tape";
                    return true;
                case "stamps":
                case "stamp":
                    itemType = "stamps";
                    return true;
                default:
                    return false;
            }
        }

        private string GetDisplayItemName(string itemType)
        {
            switch (itemType)
            {
                case "paper":
                    return "Paper";
                case "bubble wrap":
                    return "Bubble Wrap";
                case "box":
                    return "Box";
                case "envelop":
                    return "Envelope";
                case "tape":
                    return "Tape";
                case "stamps":
                    return "Stamps";
                default:
                    return itemType;
            }
        }

        private void ShowAlert(string message)
        {
            string safeMessage = message.Replace("\\", "\\\\").Replace("'", "\\'");
            ClientScript.RegisterStartupScript(GetType(), "storeAlert", $"alert('{safeMessage}');", true);
        }
    }
}
