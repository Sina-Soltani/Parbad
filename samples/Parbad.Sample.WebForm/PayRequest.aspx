<%@ Page Title="Request" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PayRequest.aspx.cs" Inherits="Parbad.Sample.WebForm.PayRequest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Request</h1>
    
    <div class="form-group">
        <asp:Label ID="Label1" runat="server" Text="Tracking number" CssClass="control-label"></asp:Label>
        <asp:TextBox ID="TxtTrackingNumber" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
    <div class="form-group form-check">
        <asp:CheckBox ID="GenerateTrackingNumberAutomatically" runat="server" CssClass="form-check-input" />
        <asp:Label ID="Label4" runat="server" Text="Generate the Tracking number automatically?" CssClass="control-label"></asp:Label>
    </div>
    <div class="form-group">
        <asp:Label ID="Label2" runat="server" Text="Amount" CssClass="control-label"></asp:Label>
        <asp:TextBox ID="TxtAmount" runat="server" CssClass="form-control"></asp:TextBox>
    </div>
    <div class="form-group">
        <asp:Label ID="Label3" runat="server" Text="Gateway" CssClass="control-label"></asp:Label>
        <asp:DropDownList ID="DropGateway" runat="server" CssClass="form-control"></asp:DropDownList>
    </div>
    <asp:Button ID="BtnPay" CssClass="btn btn-success" runat="server" Text="Pay" OnClick="BtnPay_Click" />

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
                <td>Gateway account name</td>
                <td><asp:Label ID="LblGatewayAccountName" runat="server" Text=""></asp:Label></td>
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
