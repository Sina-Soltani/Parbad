namespace Parbad.Gateway.Pasargad
{
    public interface IPasargadCrypto
    {
        string Encrypt(string privateKey, string data);
    }
}
