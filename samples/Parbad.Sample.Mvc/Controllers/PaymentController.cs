using System.Threading.Tasks;
using System.Web.Mvc;
using Parbad.Core;
using Parbad.Mvc;
using Parbad.Sample.Mvc.Models;

namespace Parbad.Sample.Mvc.Controllers
{
    public class PaymentController : Controller
    {
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

            var invoice = new Invoice(payViewModel.OrderNumber, payViewModel.Amount, verifyUrl);

            var result = await Payment.RequestAsync(payViewModel.Gateway, invoice);

            if (result.Status == RequestResultStatus.Success)
            {
                return result.ToActionResult();
            }

            return View("PayRequestResult", result);
        }

        // It's better to set no HttpMethod(HttpGet, HttpPost, etc.) for the Verify action,
        // because the banks send their information with different http methods
        // درگاه‌های بانکی، اطلاعات خود را با متد‌های مختلفی ارسال میکنند
        // بنابراین بهتر است هیچگونه خصوصیتی برای این اکشن متد در نظر گرفته نشود
        public async Task<ActionResult> Verify()
        {
            var result = await Payment.VerifyAsync(HttpContext, invoice =>
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

            return View(result);
        }

        [HttpGet]
        public ActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Refund(RefundViewModel refundViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(refundViewModel);
            }

            var result = Payment.Refund(new RefundInvoice(refundViewModel.OrderNumber, refundViewModel.Amount));

            return View("RefundResult", result);
        }

        private static bool Is_There_Still_Enough_Smartphone_In_Shop(long orderNumber, string referenceId)
        {
            // Yes, we still have smartphones :)

            return true;
        }
    }
}