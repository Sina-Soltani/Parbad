<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Parbad.Sample.WebForm._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Parbad Demo</h1>
    </div>

    <a runat="server" href="~/PayRequest" class="btn btn-primary btn-lg">Make a payment</a>
    <a runat="server" href="~/Refund" class="btn btn-primary btn-lg">Refund a payment</a>

</asp:Content>
