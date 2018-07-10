<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Parbad.Sample.WebForm._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Parbad Demo</h1>
    </div>

    <p><a runat="server" href="~/PayRequest">Request</a></p>
    <p><a runat="server" href="~/Refund">Refund</a></p>

</asp:Content>
