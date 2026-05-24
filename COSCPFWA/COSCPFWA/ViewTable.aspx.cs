using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace COSCPFWA
{
    public partial class ViewTable : System.Web.UI.Page
    {
        private string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

        // dictionary to store primary keys for each table
        public readonly Dictionary<string, string> tablePrimaryKeys = new Dictionary<string, string>
        {
            {"customer", "CustomerID" },
            {"package", "PackageID" },
            { "employee", "EmployeeID" },
            {"notifications", "NotificationID" },
            {"trackinghistory", "TrackingID" },
            {"incident", "IncidentID" },
            {"money_orders", "ServiceID" },
            {"shippingdetails", "SenderID" },
            {"inventory", "inventory_item_id" },
            {"store", "transaction_id" },
            {"goverment_services", "ServiceID" }
            // add primary keys of each table you include in the drop-down list
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTableData("customer"); // default table is customer
            }
        }

        //load data based on the selected table
        protected void LoadTableData(string tableName)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                string query = $"SELECT * FROM {tableName}";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        gvData.DataSource = dt;
                        gvData.DataBind();
                        gvData.Visible = true; // show on the GridView after data loads
                    }
                }
            }
        }

        // this handles the dropdown selection change to load data
        protected void ddlTableSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = ddlTableSelect.SelectedValue;
            if (!string.IsNullOrEmpty(selectedTable))
            {
                LoadTableData(selectedTable);
            }
        }

        // Edit the row by setting GridView into edit mode
        protected void gvData_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvData.EditIndex = e.NewEditIndex;
            LoadTableData(ddlTableSelect.SelectedValue);
        }

        // Row updating, update record in database
        /*
        protected void gvData_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvData.Rows[e.RowIndex];
            string tableName = ddlTableSelect.SelectedValue;
            string primaryKeyColumn = GetPrimaryKeyColumn(tableName);
            string rowID = ((Label)row.FindControl("lblPrimaryKey")).Text; 

            //A list to store column update statements
            List<string> updateColumns = new List<string>();
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            // Loop through each cell
            for (int i = 1; i < row.Cells.Count; i++)
            {
                // Check if the cell contains a control and if that control is a TextBox
                if (row.Cells[i].Controls.Count > 0 && row.Cells[i].Controls[0] is TextBox txtBox)
                {
                    string columnName = gvData.HeaderRow.Cells[i].Text; // Get column name from the header
                    string newValue = txtBox.Text.Trim();

                    updateColumns.Add($"{columnName} = @{columnName}");
                    updatedValues.Add(columnName, newValue);
                }
            }

            if (updateColumns.Count == 0) return; // no updates needed

            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // UPDATE query
                    string query = $"UPDATE {tableName} SET {string.Join(", ", updateColumns)} WHERE {primaryKeyColumn} = @RowID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // add param updated column
                        foreach (var kvp in updatedValues)
                        {
                            cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                        }

                        // add param for primary key
                        cmd.Parameters.AddWithValue("@RowID", rowID);

                        cmd.ExecuteNonQuery();
                    }

                    gvData.EditIndex = -1; // exit edit
                    LoadTableData(tableName); // reload data
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error updating row: " + ex.Message + "');</script>");
                }
            }
        }


        */
        // cancel edit mode
        protected void gvData_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvData.EditIndex = -1; 
            LoadTableData(ddlTableSelect.SelectedValue);
        }

        // delete row from table, deletes from database
        protected void gvData_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridViewRow row = gvData.Rows[e.RowIndex];
            string rowID = ((Label)row.FindControl("lblPrimaryKey")).Text;
            string tableName = ddlTableSelect.SelectedValue;

            string primaryKeyColumn = GetPrimaryKeyColumn(tableName); // use helper to get primary key column
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = $"DELETE FROM {tableName} WHERE {primaryKeyColumn} = @RowID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RowID", rowID);
                        cmd.ExecuteNonQuery();
                    }

                    LoadTableData(tableName); // Reload table data
                }
                catch (Exception)
                {
                    Response.Write("<script>alert('Error deleting record.');</script>");
                }
            }
        }


        // Helper function to get primary key column based on table
        private string GetPrimaryKeyColumn(string tableName)
        {
            if (tablePrimaryKeys.TryGetValue(tableName, out string primaryKey))
            {
                return primaryKey;
            }
            // handle tables without primary key
            throw new Exception($"Primary key for table '{tableName}' is not defined.");
        }
    }
}
