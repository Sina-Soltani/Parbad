using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.AspNetCore.Models
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

    public class PayViewModel
    {
        public long Amount { get; set; }

        [Display(Name = "Gateway")]
        public Gateways SelectedGateway { get; set; }
    }
}
