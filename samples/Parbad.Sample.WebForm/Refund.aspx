<%@ Page Title="Refund" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Refund.aspx.cs" Inherits="Parbad.Sample.WebForm.Refund" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Refund a payment</h1>

    <div class="form-group">
        <label class="control-label">Tracking number</label>
        <asp:TextBox ID="TxtTrackingNumber" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
    
    <asp:Button ID="BtnRefund" CssClass="btn btn-success" runat="server" Text="Refund" OnClick="BtnRefund_Click" />

    <asp:Panel ID="ResultPanel" runat="server" Visible="False">
        <h1>Result</h1>

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
                <td>Is succeed</td>
                <td><asp:Label ID="LblIsSucceed" runat="server" Text=""></asp:Label></td>
            </tr>
            <tr>
                <td>Message</td>
                <td class="text-lg"><asp:Label ID="LblMessage" runat="server" Text=""></asp:Label></td>
            </tr>
        </table>
    </asp:Panel>
</asp:Content>
