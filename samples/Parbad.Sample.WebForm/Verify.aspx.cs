using System;

namespace Parbad.Sample.WebForm
{
    public partial class Verify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var result = Payment.Verify(Context, invoice =>
            {
                // You can check your database, whether or not you have still products to sell
                // در این مرحله هنوز درخواست تأیید و واریز وجه از وب سایت شما به بانک ارسال نشده است
                // بنابراین شما می توانید اطلاعات صورتحساب را با پایگاه داده خود چک کنید
                // و در صورت لزوم تراکنش را مردود اعلام کنید

                if (!Is_There_Still_Enough_Smartphone_In_Shop(invoice.OrderNumber, invoice.ReferenceId))
                {
                    invoice.Cancel("We have no more smartphones to sell.");
                }
            });

            LblGateway.Text = result.Gateway.ToString();
            LblReferenceId.Text = result.ReferenceId;
            LblTransactionId.Text = result.TransactionId;
            LblStatus.Text = result.Status.ToString();
            LblMessage.Text = result.Message;

        }

        private static bool Is_There_Still_Enough_Smartphone_In_Shop(long orderNumber, string referenceId)
        {
            // Yes, we still have smartphones :)

            return true;
        }
    }
}