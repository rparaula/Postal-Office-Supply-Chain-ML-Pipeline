using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;

namespace COSCPFWA
{
    public partial class ShippingHistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void ViewReport_Click(object sender, EventArgs e)
        {
            int customerIdValue;
            int? customerId = null;

            if (!string.IsNullOrWhiteSpace(customerID.Text))
            {
                if (!int.TryParse(customerID.Text.Trim(), out customerIdValue) || customerIdValue <= 0)
                {
                    ShowAlert("Customer ID must be a positive whole number.");
                    return;
                }

                customerId = customerIdValue;
            }

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
                    string query = @"
                        SELECT c.customer_id,
                               CONCAT(c.first_name, ' ', c.last_name) AS customer_name,
                               p.package_id,
                               COALESCE(st.service_type_name, p.service_type) AS service_type,
                               ps.status_name AS package_status,
                               p.received_date,
                               p.shipping_cost,
                               sm.shipping_method_name,
                               CONCAT_WS(' ', sd.recipient_first_name, sd.recipient_middle_initial, sd.recipient_last_name) AS recipient_name,
                               sd.recipient_address,
                               th.tracking_id,
                               th.tracking_timestamp,
                               th.sent_date,
                               th.expected_delivery_date,
                               th.last_seen_vehicle_location,
                               e.full_name AS handled_by_employee
                        FROM package p
                        JOIN customer c ON p.customer_id = c.customer_id
                        JOIN package_status ps ON p.package_status_id = ps.package_status_id
                        LEFT JOIN service_type st ON p.service_type_id = st.service_type_id
                        LEFT JOIN shippingdetails sd ON sd.package_id = p.package_id
                        LEFT JOIN shipping_method sm ON sd.shipping_method_id = sm.shipping_method_id
                        LEFT JOIN trackinghistory th ON th.package_id = p.package_id
                        LEFT JOIN employee e ON p.employee_id = e.employee_id
                        WHERE (@CustomerID IS NULL OR c.customer_id = @CustomerID)
                        ORDER BY p.received_date DESC, th.tracking_timestamp DESC, p.package_id DESC
                        LIMIT 500";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerId.HasValue ? (object)customerId.Value : DBNull.Value);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable report = new DataTable();
                            adapter.Fill(report);
                            ResultGrid.DataSource = report;
                            ResultGrid.DataBind();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowAlert("Database error loading shipping history: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAlert("Failed to load shipping history: " + ex.Message);
                }
            }
        }

        private void ShowAlert(string message)
        {
            string safeMessage = message.Replace("\\", "\\\\").Replace("'", "\\'");
            ClientScript.RegisterStartupScript(GetType(), "shippingHistoryAlert", $"alert('{safeMessage}');", true);
        }
    }
}
