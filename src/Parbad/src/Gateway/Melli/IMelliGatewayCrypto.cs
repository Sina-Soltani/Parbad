namespace Parbad.Gateway.Melli
{
    public interface IMelliGatewayCrypto
    {
        string Encrypt(string terminalKey, string data);
    }
}
