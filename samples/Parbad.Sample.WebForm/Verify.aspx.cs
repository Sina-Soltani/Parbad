using System;

namespace Parbad.Sample.WebForm
{
    public partial class Verify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IPaymentResult result;
            var transactionCode = "";

            var invoice = StaticOnlinePayment.Instance.Fetch();

            if (Is_There_Still_Product_In_Shop(invoice.TrackingNumber))
            {
                var verifyResult = StaticOnlinePayment.Instance.Verify(invoice);
                result = verifyResult;
                transactionCode = verifyResult.TransactionCode;
            }
            else
            {
                result = StaticOnlinePayment.Instance.Cancel(invoice, cancellationReason: "Sorry, We have no more products to sell.");
            }

            LblTrackingNumber.Text = result.TrackingNumber.ToString();
            LblAmount.Text = result.Amount.ToString();
            LblGateway.Text = result.GatewayName;
            LblGatewayAccountName.Text = result.GatewayAccountName;
            LblTransactionCode.Text = transactionCode;
            LblIsSucceed.Text = result.IsSucceed.ToString();
            LblMessage.Text = result.Message;
        }

        private static bool Is_There_Still_Product_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
