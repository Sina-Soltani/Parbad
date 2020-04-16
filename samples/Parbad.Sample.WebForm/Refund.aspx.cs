﻿using System;

namespace Parbad.Sample.WebForm
{
    public partial class Refund : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected async void BtnRefund_Click(object sender, EventArgs e)
        {
            var trackingNumber = long.Parse(TxtTrackingNumber.Text);

            var result = await StaticOnlinePayment.Instance.RefundCompletelyAsync(trackingNumber);

            // Note: This is just for development and testing.
            // Don't show the actual result object to clients in production environment.
            // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
            LblTrackingNumber.Text = result.TrackingNumber.ToString();
            LblAmount.Text = result.Amount.ToString();
            LblGateway.Text = result.GatewayName;
            LblIsSucceed.Text = result.IsSucceed.ToString();
            LblStatus.Text = result.Status.ToString();
            LblMessage.Text = result.Message;
        }
    }
}
