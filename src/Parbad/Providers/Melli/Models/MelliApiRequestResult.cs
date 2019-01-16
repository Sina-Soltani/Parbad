using System;

namespace Parbad.Providers.Melli.Models
{
    [Serializable]
    internal class MelliApiRequestResult
    {
        public int ResCode { get; set; }

        public string Token { get; set; }

        public string Description { get; set; }
    }
}