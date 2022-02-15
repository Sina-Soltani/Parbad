namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalResponseModel
    {
        public long TrackId { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
        public string PayLink { get; set; }
    }
}