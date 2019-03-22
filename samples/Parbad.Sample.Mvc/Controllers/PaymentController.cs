using System.Threading.Tasks;
using System.Web.Mvc;
using Parbad.Mvc;
using Parbad.Sample.Mvc.Models;

namespace Parbad.Sample.Mvc.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IOnlinePayment _onlinePayment;

        public PaymentController(IOnlinePayment onlinePayment)
        {
            _onlinePayment = onlinePayment;
        }

        [HttpGet]
        public ActionResult PayRequest()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PayRequest(RequestViewModel payViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(payViewModel);
            }

            var verifyUrl = Url.Action("Verify", "Payment", null, Request.Url.Scheme);

            var result = await _onlinePayment.RequestAsync(invoice =>
            {
                invoice
                    .UseAutoIncrementTrackingNumber()
                    .SetAmount(payViewModel.Amount)
                    .SetCallbackUrl(verifyUrl)
                    .UseGateway(payViewModel.Gateway);
            });

            if (result.IsSucceed)
            {
                return result.GatewayTransporter.TransportToGateway();
            }

            // Note: This is just for development and testing.
            // Don't show the actual result object to clients in production environment.
            // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
            return View("PayRequestError", result);
        }

        // It's better to set no HttpMethods(HttpGet, HttpPost, etc.) for the Verify action,
        // because the banks send their information with different http methods
        // درگاه‌های بانکی، اطلاعات خود را با متد‌های مختلفی ارسال میکنند
        // بنابراین بهتر است هیچگونه خصوصیتی برای این اکشن متد در نظر گرفته نشود
        public async Task<ActionResult> Verify()
        {
            var result = await _onlinePayment.VerifyAsync(invoice =>
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
            return View(result);
        }

        [HttpGet]
        public ActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Refund(RefundViewModel refundViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(refundViewModel);
            }

            var result = await _onlinePayment.RefundCompletelyAsync(refundViewModel.TrackingNumber);

            // Note: This is just for development and testing.
            // Don't show the actual result object to clients in production environment.
            // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
            return View("RefundResult", result);
        }

        private static bool Is_There_Still_Enough_SmartPhone_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
