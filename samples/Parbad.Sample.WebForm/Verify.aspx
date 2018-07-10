<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Verify.aspx.cs" Inherits="Parbad.Sample.WebForm.Verify" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Verify result</h1>

    <table class="table">
        <tr>
            <td>Gateway</td>
            <td><asp:Label ID="LblGateway" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Reference Id</td>
            <td><asp:Label ID="LblReferenceId" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Transaction Id</td>
            <td><asp:Label ID="LblTransactionId" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Status</td>
            <td><asp:Label ID="LblStatus" runat="server" Text=""></asp:Label></td>
        </tr>
        <tr>
            <td>Message</td>
            <td class="text-lg"><asp:Label ID="LblMessage" runat="server" Text=""></asp:Label></td>
        </tr>
    </table>
</asp:Content>
