using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parbad.AspNetCore;
using Parbad.Sample.AspNetCore.Models;

namespace Parbad.Sample.AspNetCore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IOnlinePayment _onlinePayment;

        public PaymentController(IOnlinePayment onlinePayment)
        {
            _onlinePayment = onlinePayment;
        }

        [HttpGet]
        public IActionResult Pay()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(PayViewModel viewModel)
        {
            var verifyUrl = Url.Action("Verify", "Payment", null, Request.Scheme);

            var result = await _onlinePayment.RequestAsync(invoice =>
            {
                invoice
                    .UseAutoIncrementTrackingNumber()
                    .SetAmount(viewModel.Amount)
                    .SetCallbackUrl(verifyUrl)
                    .UseGateway(viewModel.SelectedGateway);
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
        public async Task<IActionResult> Verify()
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
        public IActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Refund(RefundViewModel viewModel)
        {
            var result = await _onlinePayment.RefundCompletelyAsync(viewModel.TrackingNumber);

            // Note: This is just for development and testing.
            // Don't show the actual result object to clients in production environment.
            // Instead, show only the important information such as IsSucceed, Tracking Number and Transaction Code.
            return View("RefundResult", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static bool Is_There_Still_Enough_SmartPhone_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
