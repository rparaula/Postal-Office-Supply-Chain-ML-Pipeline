using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.UI.WebControls;

namespace COSCPFWA
{
    public partial class ViewTable : System.Web.UI.Page
    {
        private const string DefaultTable = "customer";

        private string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;

        public readonly Dictionary<string, string> tablePrimaryKeys = new Dictionary<string, string>
        {
            { "customer", "customer_id" },
            { "employee", "employee_id" },
            { "departments", "department_id" },
            { "postaloffice", "postal_office_id" },
            { "package", "package_id" },
            { "package_status", "package_status_id" },
            { "service_type", "service_type_id" },
            { "shipping_method", "shipping_method_id" },
            { "shippingdetails", "package_id" },
            { "pickupdetails", "package_id" },
            { "notifications", "notification_id" },
            { "trackinghistory", "tracking_id" },
            { "incident", "incident_id" },
            { "incident_type", "incident_type_id" },
            { "incident_severity", "incident_severity_id" },
            { "incident_status", "incident_status_id" },
            { "refunds", "refund_id" },
            { "inventory", "inventory_item_id" },
            { "store", "transaction_id" },
            { "smartlocker", "locker_id" },
            { "lockerassignment", "locker_assignment_id" },
            { "lockerlocations", "locker_location_id" },
            { "package_to_locker", "package_id" },
            { "roles", "role_id" }
        };

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            gvData.PageIndexChanging += gvData_PageIndexChanging;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTableData(DefaultTable);
            }
        }

        protected void ddlTableSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvData.PageIndex = 0;
            LoadTableData(ddlTableSelect.SelectedValue);
        }

        protected void gvData_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvData.PageIndex = e.NewPageIndex;
            LoadTableData(ddlTableSelect.SelectedValue);
        }

        protected void gvData_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string tableName = ddlTableSelect.SelectedValue;
            string primaryKeyColumn;
            if (!tablePrimaryKeys.TryGetValue(tableName, out primaryKeyColumn))
            {
                ShowAlert("Selected table is not available in the v2 schema viewer.");
                return;
            }

            if (gvData.DataKeys == null || gvData.DataKeys[e.RowIndex] == null)
            {
                ShowAlert("Unable to identify the selected row.");
                return;
            }

            object rowID = gvData.DataKeys[e.RowIndex].Value;
            if (rowID == null || rowID == DBNull.Value)
            {
                ShowAlert("Unable to identify the selected row.");
                return;
            }

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
                    string query = string.Format(
                        "DELETE FROM `{0}` WHERE `{1}` = @RowID",
                        tableName,
                        primaryKeyColumn);

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RowID", rowID);
                        cmd.ExecuteNonQuery();
                    }

                    LoadTableData(tableName);
                }
                catch (MySqlException ex)
                {
                    ShowAlert("Database error deleting record: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAlert("Error deleting record: " + ex.Message);
                }
            }
        }

        protected void LoadTableData(string tableName)
        {
            string primaryKeyColumn;
            if (!tablePrimaryKeys.TryGetValue(tableName, out primaryKeyColumn))
            {
                ShowAlert("Selected table is not available in the v2 schema viewer.");
                return;
            }

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
                    string query = string.Format(
                        "SELECT * FROM `{0}` ORDER BY `{1}` LIMIT 500",
                        tableName,
                        primaryKeyColumn);

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        gvData.DataKeyNames = new[] { primaryKeyColumn };
                        gvData.DataSource = dt;
                        gvData.DataBind();
                        gvData.Visible = true;
                    }
                }
                catch (MySqlException ex)
                {
                    ShowAlert("Database error loading table: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAlert("Error loading table: " + ex.Message);
                }
            }
        }

        private void ShowAlert(string message)
        {
            string safeMessage = message.Replace("\\", "\\\\").Replace("'", "\\'");
            ClientScript.RegisterStartupScript(GetType(), "viewTableAlert", $"alert('{safeMessage}');", true);
        }
    }
}
