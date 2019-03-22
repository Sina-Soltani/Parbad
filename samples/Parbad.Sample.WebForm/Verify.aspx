<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Verify.aspx.cs" Inherits="Parbad.Sample.WebForm.Verify" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Verify result</h1>
    
    <table class="table">
        <tr>
            <td>Tracking number</td>
            <td><asp:Label ID="LblTrackingNumber" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Amount</td>
            <td><asp:Label ID="LblAmount" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Gateway</td>
            <td><asp:Label ID="LblGateway" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Transaction code</td>
            <td><asp:Label ID="LblTransactionCode" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Is succeed</td>
            <td><asp:Label ID="LblIsSucceed" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Message</td>
            <td class="text-lg"><asp:Label ID="LblMessage" runat="server" Text=""></asp:Label></td>
        </tr>
    </table>
</asp:Content>
