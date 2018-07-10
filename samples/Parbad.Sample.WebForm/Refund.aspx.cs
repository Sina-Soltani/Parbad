using System;
using Parbad.Core;

namespace Parbad.Sample.WebForm
{
    public partial class Refund : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void BtnRefund_Click(object sender, EventArgs e)
        {
            var result = Payment.Refund(new RefundInvoice(long.Parse(TxtOrderNumber.Text), long.Parse(TxtAmount.Text)));

            LblGateway.Text = result.Gateway.ToString();
            LblAmount.Text = result.Amount.ToString();
            LblStatus.Text = result.Status.ToString();
            LblMessage.Text = result.Message;
        }
    }
}