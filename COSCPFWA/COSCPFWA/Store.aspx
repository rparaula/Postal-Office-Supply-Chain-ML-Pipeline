<%@ Page Title="Store" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Store.aspx.cs" Inherits="COSCPFWA.Store" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Welcome to the Store</h1> 
    <label for="item">Select an Item:</label>
    <select id="item" name="item" required>
        <option value="paper">Paper</option>
        <option value="bubble wrap">Bubble Wrap</option>
        <option value="envelop">Envelope</option>
        <option value="box">Box</option>
        <option value="stamps">Stamps</option>
        <option value="tape">Tape</option>
    </select>
    <br /><br /> 
    <label for="quantity">Enter Quantity:</label>
    <input type="number" id="quantity" name="quantity" min="1" max="100" required />
    <br /><br /> 
    <input type="submit" value="Submit" />
&nbsp;<style type="text/css">
        #item {
            height: 23px;
            width: 133px;
        }
    </style>
</asp:Content>
