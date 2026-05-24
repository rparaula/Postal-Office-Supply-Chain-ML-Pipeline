using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace COSCPFWA
{
    public partial class SubscribeSmartLocker : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadLockerLocations();
                lblMessage.Visible = false;
            }

            if (IsPostBack && Request.Form["confirmPayment"] == "true")
            {
                ConfirmPayment();
            }
        }

        private void LoadLockerLocations()
        {
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;
            ddlLockerLocation.Items.Clear();
            ddlLockerLocation.Items.Add(new ListItem("Select Location", ""));

            if (string.IsNullOrEmpty(connString))
            {
                ShowMessage("Database connection string is missing or misconfigured.", false);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT ll.locker_location_id,
                               ll.location_name,
                               SUM(CASE WHEN sl.locker_status = 'Available' THEN 1 ELSE 0 END) AS available_lockers
                        FROM lockerlocations ll
                        LEFT JOIN smartlocker sl ON sl.locker_location_id = ll.locker_location_id
                        GROUP BY ll.locker_location_id, ll.location_name
                        ORDER BY ll.location_name";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int availableLockers = reader["available_lockers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["available_lockers"]);
                            string text = string.Format(
                                "{0} ({1} available)",
                                reader["location_name"],
                                availableLockers);

                            ddlLockerLocation.Items.Add(new ListItem(text, Convert.ToString(reader["locker_location_id"])));
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowMessage("Database error loading locker locations: " + ex.Message, false);
                }
                catch (Exception ex)
                {
                    ShowMessage("Failed to load locker locations: " + ex.Message, false);
                }
            }
        }

        private void ConfirmPayment()
        {
            int lockerLocationId;
            string selectedValue = Request.Form[ddlLockerLocation.UniqueID] ?? ddlLockerLocation.SelectedValue;

            if (!int.TryParse(selectedValue, out lockerLocationId) || lockerLocationId <= 0)
            {
                ShowMessage("Please select a locker location.", false);
                return;
            }

            string message;
            if (SubscribeCustomerToLocker(lockerLocationId, out message))
            {
                ShowMessage(message, true);
                LoadLockerLocations();
            }
            else
            {
                ShowMessage(message, false);
            }
        }

        private bool SubscribeCustomerToLocker(int lockerLocationId, out string message)
        {
            message = "Failed to subscribe. Please try again later.";

            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;
            if (string.IsNullOrEmpty(connString))
            {
                message = "Database connection string is missing or misconfigured.";
                return false;
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
                            int? customerId = GetCurrentCustomerId(conn, transaction);
                            if (!customerId.HasValue)
                            {
                                message = "Please create or load customer information before subscribing to a SmartLocker.";
                                transaction.Rollback();
                                return false;
                            }

                            LockerSelection locker = GetAvailableLocker(conn, transaction, lockerLocationId);
                            if (locker == null)
                            {
                                message = "No available lockers were found at that location.";
                                transaction.Rollback();
                                return false;
                            }

                            string insertQuery = @"
                                INSERT INTO lockerassignment
                                    (locker_id, customer_id, assigned_at, expires_at)
                                VALUES
                                    (@LockerID, @CustomerID, CURRENT_TIMESTAMP, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 30 DAY))";

                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@LockerID", locker.LockerId);
                                insertCmd.Parameters.AddWithValue("@CustomerID", customerId.Value);
                                insertCmd.ExecuteNonQuery();
                            }

                            string updateQuery = @"
                                UPDATE smartlocker
                                SET locker_status = 'Occupied'
                                WHERE locker_id = @LockerID";

                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@LockerID", locker.LockerId);
                                updateCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            message = "You have successfully subscribed to the SmartLocker at " + locker.LocationName + ".";
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    message = "Database error subscribing to SmartLocker: " + ex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    message = "Failed to subscribe to SmartLocker: " + ex.Message;
                    return false;
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

        private LockerSelection GetAvailableLocker(MySqlConnection conn, MySqlTransaction transaction, int lockerLocationId)
        {
            string query = @"
                SELECT sl.locker_id, ll.location_name
                FROM smartlocker sl
                JOIN lockerlocations ll ON sl.locker_location_id = ll.locker_location_id
                WHERE sl.locker_location_id = @LockerLocationID
                  AND sl.locker_status = 'Available'
                ORDER BY sl.locker_id
                LIMIT 1
                FOR UPDATE";

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LockerLocationID", lockerLocationId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new LockerSelection
                    {
                        LockerId = Convert.ToInt32(reader["locker_id"]),
                        LocationName = Convert.ToString(reader["location_name"])
                    };
                }
            }
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            lblMessage.Visible = true;
        }

        private class LockerSelection
        {
            public int LockerId { get; set; }
            public string LocationName { get; set; }
        }
    }
}
