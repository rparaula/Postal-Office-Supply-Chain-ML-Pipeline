<%@ Page Title="View Table" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ViewTable.aspx.cs" Inherits="COSCPFWA.ViewTable" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h1>View and Manage Tables</h1>

        <div class="mt-3">
            <label for="ddlTableSelect">Select Table:</label>
            <asp:DropDownList ID="ddlTableSelect" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTableSelect_SelectedIndexChanged">
                <asp:ListItem Text="Customer" Value="customer" />
                <asp:ListItem Text="Employee" Value="employee" />
                <asp:ListItem Text="Departments" Value="departments" />
                <asp:ListItem Text="Postal Office" Value="postaloffice" />
                <asp:ListItem Text="Package" Value="package" />
                <asp:ListItem Text="Package Status" Value="package_status" />
                <asp:ListItem Text="Service Type" Value="service_type" />
                <asp:ListItem Text="Shipping Method" Value="shipping_method" />
                <asp:ListItem Text="Shipping Details" Value="shippingdetails" />
                <asp:ListItem Text="Pickup Details" Value="pickupdetails" />
                <asp:ListItem Text="Notifications" Value="notifications" />
                <asp:ListItem Text="Tracking History" Value="trackinghistory" />
                <asp:ListItem Text="Incident" Value="incident" />
                <asp:ListItem Text="Incident Type" Value="incident_type" />
                <asp:ListItem Text="Incident Severity" Value="incident_severity" />
                <asp:ListItem Text="Incident Status" Value="incident_status" />
                <asp:ListItem Text="Refunds" Value="refunds" />
                <asp:ListItem Text="Inventory" Value="inventory" />
                <asp:ListItem Text="Store" Value="store" />
                <asp:ListItem Text="Smart Locker" Value="smartlocker" />
                <asp:ListItem Text="Locker Assignments" Value="lockerassignment" />
                <asp:ListItem Text="Locker Locations" Value="lockerlocations" />
                <asp:ListItem Text="Package To Locker" Value="package_to_locker" />
                <asp:ListItem Text="Roles" Value="roles" />
            </asp:DropDownList>
        </div>

        <div class="table-responsive mt-4" style="overflow-x: auto;">
            <asp:GridView ID="gvData" runat="server" AutoGenerateColumns="true" CssClass="table table-striped" 
                          OnRowDeleting="gvData_RowDeleting" AllowPaging="true" PageSize="10">
                <Columns>
                    <asp:CommandField ShowDeleteButton="true" />
                </Columns>
            </asp:GridView>
        </div>
    </main>
</asp:Content>
