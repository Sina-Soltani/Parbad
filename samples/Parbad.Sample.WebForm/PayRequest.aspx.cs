﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parbad.Sample.WebForm.Models;

namespace Parbad.Sample.WebForm
{
    public partial class PayRequest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FillDropDownList();
            }
        }

        protected async void BtnPay_Click(object sender, EventArgs e)
        {
            var verifyUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}/Verify";

            var gateway = (Gateways)long.Parse(DropGateway.SelectedValue);

            var result = await StaticOnlinePayment.Instance.RequestAsync(invoice =>
            {
                invoice
                    .SetAmount(long.Parse(TxtAmount.Text))
                    .SetCallbackUrl(verifyUrl)
                    .SetGateway(gateway.ToString());

                if (GenerateTrackingNumberAutomatically.Checked)
                {
                    invoice.UseAutoIncrementTrackingNumber();
                }
                else
                {
                    invoice.SetTrackingNumber(long.Parse(TxtTrackingNumber.Text));
                }
            });

            if (result.IsSucceed)
            {
                await result.GatewayTransporter.TransportAsync();
            }
            else
            {
                ResultPanel.Visible = true;

                // Note: This is just for development and testing.
                // Don't show the actual result object to clients in production environment.
                // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
                LblTrackingNumber.Text = result.TrackingNumber.ToString();
                LblAmount.Text = result.Amount.ToString();
                LblGateway.Text = result.GatewayName;
                LblGatewayAccountName.Text = result.GatewayAccountName;
                LblIsSucceed.Text = result.IsSucceed.ToString();
                LblStatus.Text = result.Status.ToString();
                LblMessage.Text = result.Message;
            }
        }

        public void FillDropDownList()
        {
            var values = Enum.GetValues(typeof(Gateways)).Cast<Gateways>();

            var items = new Dictionary<byte, string>();

            foreach (var gateway in values)
            {
                items.Add(Convert.ToByte(gateway), gateway.ToString());
            }

            DropGateway.DataSource = items;
            DropGateway.DataTextField = "Value";
            DropGateway.DataValueField = "Key";
            DropGateway.DataBind();
        }
    }
}
