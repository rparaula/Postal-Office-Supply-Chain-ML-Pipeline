using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;

namespace COSCPFWA
{
    public partial class DataReportRequest : System.Web.UI.Page
    {
        private const string CustomerReport = "Customer";
        private const string EmployeeReport = "Employee";
        private const string RevenueReport = "Revenue";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateOrderByDropdown();
            }
        }

        protected void btnGenerateChart_Click(object sender, EventArgs e)
        {
            GenerateChart();
        }

        protected void ViewReport_Click(object sender, EventArgs e)
        {
            ReportFilters filters;
            if (!TryGetFilters(out filters))
            {
                return;
            }

            string connString = GetConnectionString();
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
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = filters.ReportType == RevenueReport
                            ? BuildRevenueReportQuery(cmd, filters)
                            : BuildShippingReportQuery(cmd, filters);

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
                    ShowAlert("Database error generating report: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAlert("Failed to generate report: " + ex.Message);
                }
            }
        }

        private void PopulateOrderByDropdown()
        {
            if (orderByDropdown.Items.Count == 0)
            {
                orderByDropdown.Items.Add(new ListItem("Service Type", "service_type"));
                orderByDropdown.Items.Add(new ListItem("Package ID", "package_id"));
                orderByDropdown.Items.Add(new ListItem("Received Date", "received_date"));
                orderByDropdown.Items.Add(new ListItem("Package Status", "package_status"));
            }

            if (DropDownList4.Items.Count == 0)
            {
                DropDownList4.Items.Add(new ListItem("Service Type", "service_type"));
                DropDownList4.Items.Add(new ListItem("Package ID", "package_id"));
                DropDownList4.Items.Add(new ListItem("Received Date", "received_date"));
                DropDownList4.Items.Add(new ListItem("Package Status", "package_status"));
            }
        }

        private void GenerateChart()
        {
            ReportFilters filters;
            if (!TryGetFilters(out filters))
            {
                return;
            }

            string connString = GetConnectionString();
            if (string.IsNullOrEmpty(connString))
            {
                ShowAlert("Database connection string is missing or misconfigured.");
                return;
            }

            List<string> labels = new List<string>();
            List<decimal> values = new List<decimal>();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = BuildChartQuery(cmd, filters);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                labels.Add(Convert.ToString(reader["service_type"]));
                                values.Add(Convert.ToDecimal(reader["metric_value"]));
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowAlert("Database error generating chart: " + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    ShowAlert("Failed to generate chart: " + ex.Message);
                    return;
                }
            }

            var jsonData = new
            {
                labels = labels,
                values = values
            };

            chartData.Value = JsonConvert.SerializeObject(jsonData);
            HiddenField1.Value = chartData.Value;
        }

        private string BuildShippingReportQuery(MySqlCommand cmd, ReportFilters filters)
        {
            StringBuilder query = new StringBuilder(@"
                SELECT c.customer_id,
                       c.first_name,
                       c.last_name,
                       p.package_id,
                       COALESCE(st.service_type_name, p.service_type) AS service_type,
                       ps.status_name AS package_status,
                       p.received_date,
                       p.shipping_cost,
                       e.full_name AS handled_by_employee,
                       sm.shipping_method_name,
                       sd.recipient_address,
                       th.tracking_timestamp,
                       th.last_seen_vehicle_location
                FROM package p
                JOIN customer c ON p.customer_id = c.customer_id
                JOIN package_status ps ON p.package_status_id = ps.package_status_id
                LEFT JOIN service_type st ON p.service_type_id = st.service_type_id
                LEFT JOIN employee e ON p.employee_id = e.employee_id
                LEFT JOIN shippingdetails sd ON sd.package_id = p.package_id
                LEFT JOIN shipping_method sm ON sd.shipping_method_id = sm.shipping_method_id
                LEFT JOIN trackinghistory th ON th.package_id = p.package_id
                WHERE 1 = 1");

            ApplyFilters(query, cmd, filters);
            query.AppendLine(" ORDER BY p.received_date DESC, p.package_id DESC LIMIT 500");
            return query.ToString();
        }

        private string BuildRevenueReportQuery(MySqlCommand cmd, ReportFilters filters)
        {
            StringBuilder query = new StringBuilder(@"
                SELECT COALESCE(st.service_type_name, p.service_type) AS service_type,
                       COUNT(*) AS package_count,
                       COALESCE(SUM(p.shipping_cost), 0) AS total_shipping_cost,
                       COALESCE(AVG(p.shipping_cost), 0) AS average_shipping_cost
                FROM package p
                JOIN customer c ON p.customer_id = c.customer_id
                LEFT JOIN service_type st ON p.service_type_id = st.service_type_id
                LEFT JOIN employee e ON p.employee_id = e.employee_id
                WHERE 1 = 1");

            ApplyFilters(query, cmd, filters);
            query.AppendLine(" GROUP BY COALESCE(st.service_type_name, p.service_type)");
            query.AppendLine(" ORDER BY total_shipping_cost DESC");
            return query.ToString();
        }

        private string BuildChartQuery(MySqlCommand cmd, ReportFilters filters)
        {
            bool revenueMetric = filters.ReportType == RevenueReport;

            StringBuilder query = new StringBuilder(@"
                SELECT COALESCE(st.service_type_name, p.service_type) AS service_type,");

            query.AppendLine(revenueMetric
                ? "       COALESCE(SUM(p.shipping_cost), 0) AS metric_value"
                : "       COUNT(*) AS metric_value");

            query.AppendLine(@"
                FROM package p
                JOIN customer c ON p.customer_id = c.customer_id
                LEFT JOIN service_type st ON p.service_type_id = st.service_type_id
                LEFT JOIN employee e ON p.employee_id = e.employee_id
                WHERE 1 = 1");

            ApplyFilters(query, cmd, filters);
            query.AppendLine(" GROUP BY COALESCE(st.service_type_name, p.service_type)");
            query.AppendLine(revenueMetric ? " ORDER BY metric_value DESC" : " ORDER BY service_type");
            return query.ToString();
        }

        private void ApplyFilters(StringBuilder query, MySqlCommand cmd, ReportFilters filters)
        {
            if (!string.IsNullOrEmpty(filters.PackageType))
            {
                query.AppendLine(" AND (st.service_type_name = @PackageType OR p.service_type = @PackageType)");
                cmd.Parameters.AddWithValue("@PackageType", filters.PackageType);
            }

            if (!string.IsNullOrEmpty(filters.CustomerFirstName))
            {
                query.AppendLine(" AND c.first_name LIKE @CustomerFirstName");
                cmd.Parameters.AddWithValue("@CustomerFirstName", filters.CustomerFirstName + "%");
            }

            if (!string.IsNullOrEmpty(filters.CustomerLastName))
            {
                query.AppendLine(" AND c.last_name LIKE @CustomerLastName");
                cmd.Parameters.AddWithValue("@CustomerLastName", filters.CustomerLastName + "%");
            }

            if (!string.IsNullOrEmpty(filters.AdditionalCustomer))
            {
                query.AppendLine(" AND CONCAT(c.first_name, ' ', c.last_name) LIKE @AdditionalCustomer");
                cmd.Parameters.AddWithValue("@AdditionalCustomer", "%" + filters.AdditionalCustomer + "%");
            }

            if (!string.IsNullOrEmpty(filters.EmployeeName))
            {
                query.AppendLine(" AND e.full_name LIKE @EmployeeName");
                cmd.Parameters.AddWithValue("@EmployeeName", "%" + filters.EmployeeName + "%");
            }

            if (!string.IsNullOrEmpty(filters.AdditionalEmployee))
            {
                query.AppendLine(" AND e.full_name LIKE @AdditionalEmployee");
                cmd.Parameters.AddWithValue("@AdditionalEmployee", "%" + filters.AdditionalEmployee + "%");
            }

            if (filters.DateFrom.HasValue)
            {
                query.AppendLine(" AND p.received_date >= @DateFrom");
                cmd.Parameters.AddWithValue("@DateFrom", filters.DateFrom.Value);
            }

            if (filters.DateToExclusive.HasValue)
            {
                query.AppendLine(" AND p.received_date < @DateToExclusive");
                cmd.Parameters.AddWithValue("@DateToExclusive", filters.DateToExclusive.Value);
            }
        }

        private bool TryGetFilters(out ReportFilters filters)
        {
            filters = new ReportFilters
            {
                ReportType = reportType.SelectedValue
            };

            if (string.IsNullOrEmpty(filters.ReportType))
            {
                ShowAlert("Please select a report type.");
                return false;
            }

            if (filters.ReportType == CustomerReport || filters.ReportType == RevenueReport)
            {
                filters.CustomerFirstName = customerFirstName.Text.Trim();
                filters.CustomerLastName = customerLastName.Text.Trim();
                filters.AdditionalCustomer = additionalCustomer.Text.Trim();
                filters.PackageType = packageType.SelectedValue;

                ReportFilters dateFilters;
                if (!TryReadDateRange(activityDateFrom.Text, activityDateTo.Text, out dateFilters))
                {
                    return false;
                }

                filters.DateFrom = dateFilters.DateFrom;
                filters.DateToExclusive = dateFilters.DateToExclusive;
            }
            else if (filters.ReportType == EmployeeReport)
            {
                filters.EmployeeName = employeeName.Text.Trim();
                filters.AdditionalEmployee = TextBox1.Text.Trim();
                filters.PackageType = DropDownList1.SelectedValue;

                ReportFilters dateFilters;
                if (!TryReadDateRange(TextBox2.Text, TextBox3.Text, out dateFilters))
                {
                    return false;
                }

                filters.DateFrom = dateFilters.DateFrom;
                filters.DateToExclusive = dateFilters.DateToExclusive;
            }
            else
            {
                ShowAlert("Please select a valid report type.");
                return false;
            }

            return true;
        }

        private bool TryReadDateRange(string fromText, string toText, out ReportFilters dateFilters)
        {
            dateFilters = new ReportFilters();

            DateTime parsedDate;
            if (!string.IsNullOrWhiteSpace(fromText))
            {
                if (!DateTime.TryParseExact(fromText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    ShowAlert("The From date is invalid.");
                    return false;
                }

                dateFilters.DateFrom = parsedDate.Date;
            }

            if (!string.IsNullOrWhiteSpace(toText))
            {
                if (!DateTime.TryParseExact(toText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    ShowAlert("The To date is invalid.");
                    return false;
                }

                dateFilters.DateToExclusive = parsedDate.Date.AddDays(1);
            }

            if (dateFilters.DateFrom.HasValue &&
                dateFilters.DateToExclusive.HasValue &&
                dateFilters.DateFrom.Value >= dateFilters.DateToExclusive.Value)
            {
                ShowAlert("The From date must be before or equal to the To date.");
                return false;
            }

            return true;
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DataBaseConnectionString"]?.ConnectionString;
        }

        private void ShowAlert(string message)
        {
            string safeMessage = message.Replace("\\", "\\\\").Replace("'", "\\'");
            ClientScript.RegisterStartupScript(GetType(), "dataReportAlert", $"alert('{safeMessage}');", true);
        }

        private class ReportFilters
        {
            public string ReportType { get; set; }
            public string CustomerFirstName { get; set; }
            public string CustomerLastName { get; set; }
            public string AdditionalCustomer { get; set; }
            public string EmployeeName { get; set; }
            public string AdditionalEmployee { get; set; }
            public string PackageType { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateToExclusive { get; set; }
        }
    }
}
