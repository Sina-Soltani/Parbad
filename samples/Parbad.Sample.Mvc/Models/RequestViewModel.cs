namespace Parbad.Sample.Mvc.Models
{
    public enum Gateways
    {
        Saman,
        Mellat,
        Parsian,
        Pasargad,
        IranKish,
        Melli,
        AsanPardakht,
        ZarinPal,
        ParbadVirtual
    }

    public class RequestViewModel
    {
        public long Amount { get; set; }

        public Gateways Gateway { get; set; }
    }
}
