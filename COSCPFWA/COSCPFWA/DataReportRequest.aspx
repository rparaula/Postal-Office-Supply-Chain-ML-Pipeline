<%@ Page Title="DataReportRequest" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DataReportRequest.aspx.cs" Inherits="COSCPFWA.DataReportRequest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f2f2f2;
            margin: 0;
        }

        .main-container {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 20px;
        }

        .report-form-container {
            width: 100%;
            max-width: 600px;
            background-color: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
            border-top: 8px solid #3b3b3b;
            box-sizing: border-box;
        }

            .report-form-container h2 {
                text-align: center;
                color: #333;
                font-size: 24px;
                font-weight: bold;
                margin-bottom: 20px;
            }

        .form-group {
            display: flex;
            flex-direction: column;
            width: 100%;
            margin-bottom: 15px;
            align-items: center;
        }

            .form-group label {
                display: block;
                width: 100%;
                color: #3b3b3b;
                font-weight: bold;
                margin-bottom: 5px;
                margin-left: 260px;
            }

            .form-group input,
            .form-group select,
            .form-group asp\\:TextBox,
            .form-group asp\\:DropDownList {
                width: 100%;
                padding: 12px;
                border: 1px solid #ccc;
                border-radius: 4px;
                font-size: 15px;
                box-sizing: border-box;
            }

                .form-group input:focus,
                .form-group select:focus,
                .form-group asp\\:TextBox:focus {
                    border-color: #ffc107;
                    outline: none;
                    box-shadow: 0 0 4px rgba(255, 193, 7, 0.5);
                }


        .submit-btn {
            width: 100%;
            max-width: 300px;
            padding: 14px;
            background-color: #ffc107;
            color: #3b3b3b;
            font-size: 16px;
            font-weight: bold;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            box-sizing: border-box;
            margin-left: 120px;
        }

            .submit-btn:hover {
                background-color: #d39e00;
            }
    </style>

    
    <div class="main-container">
        <div class="report-form-container">
            <h2>Employee/Customer Report Request</h2>
            
            <div id="reportForm">

                <div class="form-group">
                    <label for="reportType">Report Type (Customer, Employee, or Revenue)</label>
                    <asp:DropDownList ID="reportType" runat="server" CssClass="form-select" required="required" AutoPostBack="false" onchange="toggleForms()">
                        <asp:ListItem Value="" Text="Select Report Type" Disabled="true" Selected="true"></asp:ListItem>
                        <asp:ListItem Value="Customer" Text="Customer"></asp:ListItem>
                        <asp:ListItem Value="Employee" Text="Employee"></asp:ListItem>
                        <asp:ListItem Value="Revenue" Text="Revenue"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- Divide visibility for different results form reportType -->
                <div id="customerReport" style="display: none;">

                    <div class="form-group">
                        <label for="customerFirstName">Customer First Name</label>
                        <asp:TextBox ID="customerFirstName" runat="server" CssClass="form-control" placeholder="Enter First Name" ></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="customerLastName">Customer Last Name</label>
                        <asp:TextBox ID="customerLastName" runat="server" CssClass="form-control" placeholder="Enter Last Name" ></asp:TextBox>
                    </div>

                
                    <div class="form-group">
                        <label for="additionalCustomer">Additional Customer</label>
                        <asp:TextBox ID="additionalCustomer" runat="server" CssClass="form-control" placeholder="Optional"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="packageType">Package Type</label>
                        <asp:DropDownList ID="packageType" runat="server" CssClass="form-select">
                            <asp:ListItem value="">Select Package Type</asp:ListItem>
                            <asp:ListItem value="Delivery">Delivery</asp:ListItem>
                            <asp:ListItem value="SmartLocker">SmartLocker</asp:ListItem>
                            <asp:ListItem value="Pickup">Pickup</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                
                    <div class="form-group">
                        <label for="activityDateFrom">Activity Date From</label>
                        <asp:TextBox ID="activityDateFrom" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                
                    <div class="form-group">
                        <label for="activityDateTo">Activity Date To</label>
                        <asp:TextBox ID="activityDateTo" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>

                    <asp:Button ID="ViewReportBtn" runat="server" Text="View Report" CssClass="submit-btn" OnClick="ViewReport_Click" />

                    <!-- Using the user inputs from all the forms before this, now generates a chart where the user gets to select the x & y axis -->
                    <h2>Data Visualization</h2>

                    <!-- X-axis -->
                    <div class="form-group">
                        <label for="xAxis">Select X-axis</label>
                        <asp:DropDownList ID="xAxis" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Service Type" Value="service_type"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <!-- Y-axis -->
                    <div class="form-group">
                        <label for="yAxis">Select Y-axis</label>
                        <asp:DropDownList ID="yAxis" runat="server" CssClass="form-select">
                            <asp:ListItem Text="# of Packages" Value="package_count"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <!-- Delete later and fix changes to C# if deleted -->
                    <label for="orderByDropdown">Order By:</label> <!-- Updated label to "Order By" -->
                    <asp:DropDownList ID="orderByDropdown" runat="server"></asp:DropDownList> <!-- Renamed to "orderByDropdown" -->

                    <!-- Generate Chart button -->
                    <asp:Button ID="btnGenerateChart" runat="server" Text="Generate Chart" OnClick="btnGenerateChart_Click" CssClass="btnStacked" />

                    <!-- Canvas element to display the generated chart -->
                    <div>
                        <canvas id="customerChart" width="400" height="200"></canvas>
                    </div>

                    <!-- Hidden field to store JSON data for chart rendering -->
                    <asp:HiddenField ID="chartData" runat="server" />

                </div>
                <!-- Employee Report Section -->
                <div id="employeeReport" style="display: none;">
                    <div class="form-group">
                        <label for="employeeName">Enter Employee Name</label>
                        <asp:TextBox ID="employeeName" runat="server" CssClass="form-control" placeholder="Enter Employee Name" ></asp:TextBox>
                    </div>

                                    
                    <div class="form-group">
                        <label for="additionalEmployees">Additional Employee</label>
                        <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" placeholder="Optional"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="packageType">Package Type</label>
                        <asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-select">
                            <asp:ListItem value="">Select Package Type</asp:ListItem>
                            <asp:ListItem value="Delivery">Delivery</asp:ListItem>
                            <asp:ListItem value="SmartLocker">SmartLocker</asp:ListItem>
                            <asp:ListItem value="Pickup">Pickup</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                
                    <div class="form-group">
                        <label for="activityDateFrom">Activity Date From</label>
                        <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                
                    <div class="form-group">
                        <label for="activityDateTo">Activity Date To</label>
                        <asp:TextBox ID="TextBox3" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>

                    <asp:Button ID="Button1" runat="server" Text="View Report" CssClass="submit-btn" OnClick="ViewReport_Click" />

                    
                    <!-- Using the user inputs from all the forms before this, now generates a chart where the user gets to select the x & y axis -->
                    <h2>Data Visualization</h2>

                    <!-- X-axis -->
                    <div class="form-group">
                        <label for="xAxis">Select X-axis</label>
                        <asp:DropDownList ID="DropDownList2" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Service Type" Value="service_type"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <!-- Y-axis -->
                    <div class="form-group">
                        <label for="yAxis">Select Y-axis</label>
                        <asp:DropDownList ID="DropDownList3" runat="server" CssClass="form-select">
                            <asp:ListItem Text="# of Packages" Value="package_count"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <!-- Delete later and fix changes to C# if deleted -->
                    <label for="orderByDropdown">Order By:</label> <!-- Updated label to "Order By" -->
                    <asp:DropDownList ID="DropDownList4" runat="server"></asp:DropDownList> <!-- Renamed to "orderByDropdown" -->

                    <!-- Generate Chart button -->
                    <asp:Button ID="Button2" runat="server" Text="Generate Chart" OnClick="btnGenerateChart_Click" CssClass="btnStacked" />

                    <!-- Canvas element to display the generated chart -->
                    <div>
                        <canvas id="employeeChart" width="400" height="200"></canvas>
                    </div>

                    <!-- Hidden field to store JSON data for chart rendering -->
                    <asp:HiddenField ID="HiddenField1" runat="server" />


                </div>
                <!-- Create revenue report section -->

                <!-- Added Chart.js library for data visualization -->
                <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

                <script type="text/javascript">
                    // Function to render chart using Chart.js and JSON data from server
                    function renderChart(data, canvasId)
                    {
                        var canvas = document.getElementById(canvasId);
                        if (!canvas) {
                            return;
                        }

                        var ctx = canvas.getContext('2d');
                        var chartData = JSON.parse(data);
                        new Chart(ctx, {
                            type: 'bar',
                            data: {
                                labels: chartData.labels,
                                datasets: [{
                                    label: '# of Packages',
                                    data: chartData.values,
                                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                                    borderColor: 'rgba(75, 192, 192, 1)',
                                    borderWidth: 1
                                }]
                            },
                            options: {
                                scales: {
                                    y: {
                                        beginAtZero: true
                                    }
                                }
                            }
                        });
                    }

                    // Toggle visibility of form sections based on report type
                    function toggleForms()
                    {
                        var reportType = document.getElementById('<%= reportType.ClientID %>').value;
                        var customerReport = document.getElementById("customerReport");
                        var employeeReport = document.getElementById("employeeReport");
                        customerReport.style.display = "none";
                        employeeReport.style.display = "none";

                        // Show relevant section based on report type
                        if (reportType === "Customer")
                        {
                            customerReport.style.display = "block";
                        }
                        else if (reportType === "Revenue")
                        {
                            customerReport.style.display = "block";
                        }
                        else if (reportType == "Employee")
                        {
                            employeeReport.style.display = "block";
                        }
                    }

                    // Initialize form toggle and chart rendering on page load
                    window.onload = function ()
                    {
                        toggleForms();
                        var chartData = document.getElementById('<%= chartData.ClientID %>').value;
                        if (chartData) {
                            var reportType = document.getElementById('<%= reportType.ClientID %>').value;
                            renderChart(chartData, reportType === "Employee" ? "employeeChart" : "customerChart");
                        }
                    };
                </script>
                
                <asp:GridView ID="ResultGrid" runat="server" AutoGenerateColumns="true" CssClass="table table-striped table-hover mt-4" />
            </div>
        </div>
    </div>
</asp:Content>


