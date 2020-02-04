using System;
using Parbad.Options;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal static class ZarinPalStatusTranslator
    {
        public static string Translate(string status, MessagesOptions messagesOptions)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            if (!int.TryParse(status, out var integerStatus))
            {
                throw new InvalidOperationException("ZarinPal error. The Status value is not an integer.");
            }

            switch (integerStatus)
            {
                case -1:
                    return "اطلاعات ارسال شده ناقص است.";
                case -2:
                    return "آی پی و يا مرچنت كد پذيرنده صحيح نيست";
                case -3:
                    return "با توجه به محدوديت هاي شاپرك امكان پرداخت با رقم درخواست شده ميسر نمي باشد.";
                case -4:
                    return "سطح تاييد پذيرنده پايين تر از سطح نقره اي است.";
                case -11:
                    return "درخواست مورد نظر يافت نشد.";
                case -12:
                    return "امكان ويرايش درخواست ميسر نمي باشد.";
                case -21:
                    return "هيچ نوع عمليات مالي براي اين تراكنش يافت نشد.";
                case -22:
                    return "تراكنش نا موفق ميباشد.";
                case -33:
                    return "رقم تراكنش با رقم پرداخت شده مطابقت ندارد.";
                case -34:
                    return "سقف تقسيم تراكنش از لحاظ تعداد يا رقم عبور نموده است";
                case -40:
                    return "اجازه دسترسي به متد مربوطه وجود ندارد.";
                case -41:
                    return "اطلاعات ارسال شده مربوط به AdditionalData غيرمعتبر ميباشد.";
                case -42:
                    return "مدت زمان معتبر طول عمر شناسه پرداخت بايد بين 30 دقيه تا 45 روز مي باشد.";
                case -54:
                    return "درخواست مورد نظر آرشيو شده است.";
                case 100:
                    return "عمليات با موفقيت انجام گرديده است.";
                case 101:
                    return "عمليات پرداخت موفق بوده و قبلا تراكنش انجام شده است.";
                default:
                    return messagesOptions.DuplicateTrackingNumber;
            }
        }
    }
}
