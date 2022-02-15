namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalRequestModel:ZibalRequest
    {
        /// <summary>
        /// تومان
        /// </summary>
        public long Amount { get; set; }
        public string CallBackUrl { get; set; }
        /// <summary>
        /// چنانچه تمایل دارید کاربر فقط از شماره کارت های مشخصی بتواند پرداخت کند لیست کارت (های) 16 رقمی را ارسال کنید.
        /// </summary>
        public string[]? AllowedCards { get; set; }
        public string OrderId { get; set; }
        public string MerchantId { get; set; }
    }
}