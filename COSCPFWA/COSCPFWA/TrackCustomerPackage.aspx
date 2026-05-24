<%@ Page Title="Track Customer Package" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TrackCustomerPackage.aspx.cs" Inherits="COSCPFWA.TrackCustomerPackage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 id="title"><%: Title %></h2>
        <div class="mt-3 mb-3">
            <asp:Label ID="PackageIdLabel" runat="server" AssociatedControlID="PackageIdTextBox" Text="Package ID" CssClass="form-label"></asp:Label>
            <asp:TextBox ID="PackageIdTextBox" runat="server" CssClass="form-control" TextMode="Number" min="1"></asp:TextBox>
        </div>
        <asp:Button ID="TrackButton" runat="server" Text="Track Package" CssClass="btn btn-primary" />
        <asp:Label ID="MessageLabel" runat="server" CssClass="d-block mt-3" Visible="false"></asp:Label>
        <asp:GridView ID="TrackingGrid" runat="server" AutoGenerateColumns="true" CssClass="table table-striped table-hover mt-4" />
    </main>
</asp:Content>
