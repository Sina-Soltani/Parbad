namespace Parbad.Providers
{
    /// <summary>
    /// Online payment providers.
    /// </summary>
    public enum Gateway
    {
        Saman = 0,
        Mellat = 1,
        Parsian = 2,
        Tejarat = 3,
        Pasargad = 4,
        /// <summary>
        /// Warning: It's not tested yet.
        /// </summary>
        IranKish = 5,
        Melli = 6,
        ParbadVirtualGateway = 255
    }
}