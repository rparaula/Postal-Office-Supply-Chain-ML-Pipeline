using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;

namespace COSCPFWA
{
    public partial class TrackCustomerPackage : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            TrackButton.Click += TrackButton_Click;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void TrackButton_Click(object sender, EventArgs e)
        {
            int packageId;
            if (!int.TryParse(PackageIdTextBox.Text.Trim(), out packageId) || packageId <= 0)
            {
                ShowMessage("Package ID must be a positive whole number.", "text-danger");
                return;
            }

            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;
            if (string.IsNullOrEmpty(connString))
            {
                ShowMessage("Database connection string is missing or misconfigured.", "text-danger");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT p.package_id,
                               CONCAT(c.first_name, ' ', c.last_name) AS customer_name,
                               COALESCE(st.service_type_name, p.service_type) AS service_type,
                               ps.status_name AS package_status,
                               p.received_date,
                               p.shipping_cost,
                               sm.shipping_method_name,
                               sd.recipient_address,
                               th.tracking_id,
                               th.tracking_timestamp,
                               th.sent_date,
                               th.expected_delivery_date,
                               th.last_seen_vehicle_location
                        FROM package p
                        JOIN customer c ON p.customer_id = c.customer_id
                        JOIN package_status ps ON p.package_status_id = ps.package_status_id
                        LEFT JOIN service_type st ON p.service_type_id = st.service_type_id
                        LEFT JOIN shippingdetails sd ON sd.package_id = p.package_id
                        LEFT JOIN shipping_method sm ON sd.shipping_method_id = sm.shipping_method_id
                        LEFT JOIN trackinghistory th ON th.package_id = p.package_id
                        WHERE p.package_id = @PackageID
                        ORDER BY th.tracking_timestamp DESC, th.tracking_id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PackageID", packageId);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable tracking = new DataTable();
                            adapter.Fill(tracking);
                            TrackingGrid.DataSource = tracking;
                            TrackingGrid.DataBind();

                            if (tracking.Rows.Count == 0)
                            {
                                ShowMessage("No package or tracking history was found for that package ID.", "text-warning");
                            }
                            else
                            {
                                ShowMessage("Tracking information loaded.", "text-success");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowMessage("Database error loading tracking information: " + ex.Message, "text-danger");
                }
                catch (Exception ex)
                {
                    ShowMessage("Failed to load tracking information: " + ex.Message, "text-danger");
                }
            }
        }

        private void ShowMessage(string message, string cssClass)
        {
            MessageLabel.Text = message;
            MessageLabel.CssClass = "d-block mt-3 " + cssClass;
            MessageLabel.Visible = true;
        }
    }
}
