using System;
using System.Configuration;
using System.Web.UI;
using MySql.Data.MySqlClient;

namespace COSCPFWA
{
    public partial class ShippingDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional: Page load logic if needed
        }

        protected void SubmitShippingDetails_Click(object sender, EventArgs e)
        {
            // Retrieve values from the form fields
            string senderAddressValue = senderAddress.Text;
            string shippingMethodValue = shippingMethod.SelectedValue;
            string receivingAddressValue = receivingAddress.Text;
            string receiverNameValue = receiverName.Text;

            // Check for empty fields (basic validation)
            if (string.IsNullOrEmpty(senderAddressValue) || string.IsNullOrEmpty(shippingMethodValue) ||
                string.IsNullOrEmpty(receivingAddressValue) || string.IsNullOrEmpty(receiverNameValue))
            {
                Response.Write("<script>alert('Please fill in all fields.');</script>");
                return;
            }

            // Retrieve the connection string from web.config
            string connString = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"]?.ConnectionString;

            if (string.IsNullOrEmpty(connString))
            {
                Response.Write("<script>alert('Database connection string is missing or misconfigured.');</script>");
                return;
            }

            // Insert data into the database
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"INSERT INTO shippingdetails 
                                     (SendingAddress, ShippingMethod, ReceivingAddress, ReceiverName) 
                                     VALUES (@SenderAddress, @ShippingMethod, @ReceivingAddress, @ReceiverName)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Set parameters to avoid SQL injection
                        cmd.Parameters.AddWithValue("@SenderAddress", senderAddressValue);
                        cmd.Parameters.AddWithValue("@ShippingMethod", shippingMethodValue);
                        cmd.Parameters.AddWithValue("@ReceivingAddress", receivingAddressValue);
                        cmd.Parameters.AddWithValue("@ReceiverName", receiverNameValue);

                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }

                    // Display success message
                    Response.Write("<script>alert('Shipping details saved successfully!');</script>");
                }
                catch (MySqlException sqlEx)
                {
                    // Specific handling for MySQL exceptions
                    Response.Write("<script>alert('Database error: " + sqlEx.Message + "');</script>");
                }
                catch (Exception ex)
                {
                    // General exception handling
                    Response.Write("<script>alert('An unexpected error occurred: " + ex.Message + "');</script>");
                }
            }
        }
    }
}
