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
        public ActionResult Pay()
        {
            return View(new RequestViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> Pay(RequestViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var verifyUrl = Url.Action("Verify", "Payment", null, Request.Url.Scheme);

            var result = await _onlinePayment.RequestAsync(invoice =>
            {
                invoice
                    .SetAmount(viewModel.Amount)
                    .SetCallbackUrl(verifyUrl)
                    .SetGateway(viewModel.Gateway.ToString());

                if (viewModel.GenerateTrackingNumberAutomatically)
                {
                    invoice.UseAutoIncrementTrackingNumber();
                }
                else
                {
                    invoice.SetTrackingNumber(viewModel.TrackingNumber);
                }
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
        // because the banks send their information with different HTTP methods
        public async Task<ActionResult> Verify()
        {
            var invoice = await _onlinePayment.FetchAsync();

            // Check if the invoice is new or it's already processed before.
            if (invoice.Status == PaymentFetchResultStatus.AlreadyProcessed)
            {
                // You can also see if the invoice is already verified before.
                var isAlreadyVerified = invoice.IsAlreadyVerified;
                return Content("The payment is already processed before.");
            }

            // An example of checking the invoice in your website.
            if (!Is_There_Still_Product_In_Shop(invoice.TrackingNumber))
            {
                var cancelResult = await _onlinePayment.CancelAsync(invoice, cancellationReason: "Sorry, We have no more products to sell.");

                return View("CancelResult", cancelResult);
            }

            var verifyResult = await _onlinePayment.VerifyAsync(invoice);

            return View(verifyResult);
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

        private static bool Is_There_Still_Product_In_Shop(long trackingNumber)
        {
            // Yes, we still have smart phones :)

            return true;
        }
    }
}
