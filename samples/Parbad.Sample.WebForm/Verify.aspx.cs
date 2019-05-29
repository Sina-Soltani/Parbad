using System;

namespace Parbad.Sample.WebForm
{
    public partial class Verify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var result = StaticOnlinePayment.Instance.Verify(invoice =>
            {
                // You can check your database, whether or not you have still products to sell
                // در این مرحله هنوز درخواست تأیید و واریز وجه از وب سایت شما به بانک ارسال نشده است
                // بنابراین شما می توانید اطلاعات صورتحساب را با پایگاه داده خود چک کنید
                // و در صورت لزوم تراکنش را لغو کنید

                if (!Is_There_Still_Enough_SmartPhone_In_Shop(invoice.TrackingNumber))
                {
                    invoice.CancelPayment("We have no more smart phones to sell.");
                }
            });

            // Note: This is just for development and testing.
            // Don't show the actual result object to clients in production environment.
            // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
            LblTrackingNumber.Text = result.TrackingNumber.ToString();
            LblAmount.Text = result.Amount.ToString();
            LblGateway.Text = result.GatewayName;
            LblGatewayAccountName.Text = result.GatewayAccountName;
            LblTransactionCode.Text = result.TransactionCode;
            LblIsSucceed.Text = result.IsSucceed.ToString();
            LblMessage.Text = result.Message;
        }

        private static bool Is_There_Still_Enough_SmartPhone_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
