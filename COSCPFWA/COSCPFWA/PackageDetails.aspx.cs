using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.UI;

namespace COSCPFWA
{
    public partial class PackageDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional: Page load logic if needed
        }

        // Check trigger and everything
        protected void SubmitPackageDetails_Click(object sender, EventArgs e)
        {
            // Retrieve form values
            string packageContents = contents.Text;
            string weightLbsValue = weightLbs.Text;
            string packageDimensions = dimensions.Text;
            string packageMaterial = material.Text;

            // Retrieve connection string from web.config
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
                    string query = @"INSERT INTO package (Contents, Weight_Lbs, Dimensions, mMaterial)
                                     VALUES (@contents, @weightLbs, @dimensions, @material)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Set parameters to avoid SQL injection
                        cmd.Parameters.AddWithValue("@Contents", packageContents);
                        cmd.Parameters.AddWithValue("@WeightLbs", weightLbs);
                        cmd.Parameters.AddWithValue("@Dimensions", packageDimensions);
                        cmd.Parameters.AddWithValue("@Material", packageMaterial);

                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }

                    // Display success message
                    Response.Write("<script>alert('Package details saved successfully.');</script>");
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
