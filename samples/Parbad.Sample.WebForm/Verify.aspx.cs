using System;

namespace Parbad.Sample.WebForm
{
    public partial class Verify : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            IPaymentResult result;
            var transactionCode = "";
            var status = "";

            var invoice = await StaticOnlinePayment.Instance.FetchAsync();

            // This is an example of cancelling an invoice when you think that the payment process must be stopped.
            if (Is_There_Still_Product_In_Shop(invoice.TrackingNumber))
            {
                var verifyResult = await StaticOnlinePayment.Instance.VerifyAsync(invoice);
                result = verifyResult;
                transactionCode = verifyResult.TransactionCode;
                status = verifyResult.Status.ToString();
            }
            else
            {
                result = await StaticOnlinePayment.Instance.CancelAsync(invoice, cancellationReason: "Sorry, We have no more products to sell.");
            }

            LblTrackingNumber.Text = result.TrackingNumber.ToString();
            LblAmount.Text = result.Amount.ToString();
            LblGateway.Text = result.GatewayName;
            LblGatewayAccountName.Text = result.GatewayAccountName;
            LblTransactionCode.Text = transactionCode;
            LblIsSucceed.Text = result.IsSucceed.ToString();
            LblStatus.Text = status;
            LblMessage.Text = result.Message;
        }

        private static bool Is_There_Still_Product_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
