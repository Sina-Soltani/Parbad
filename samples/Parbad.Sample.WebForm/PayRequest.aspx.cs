using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parbad.Core;
using Parbad.Providers;

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

        protected void BtnPay_Click(object sender, EventArgs e)
        {
            var verifyUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}/Verify";

            var invoice = new Invoice(long.Parse(TxtOrderNumber.Text), long.Parse(TxtAmount.Text), verifyUrl);

            var gateway = (Gateway)long.Parse(DropGateway.SelectedValue);

            var result = Payment.Request(gateway, invoice);

            if (result.Status == RequestResultStatus.Success)
            {
                //  This extension method, redirects the page to the gateway
                result.Process(Context);
                return;
            }

            ResultPanel.Visible = true;

            LblReferenceId.Text = result.ReferenceId;
            LblStatus.Text = result.Status.ToString();
            LblMessage.Text = result.Message;
        }

        public void FillDropDownList()
        {
            var values = Enum.GetValues(typeof(Gateway)).Cast<Gateway>();

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