using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht
{
    public interface IAsanPardakhtCrypto
    {
        Task<string> Encrypt(string input, string key, string iv);

        Task<string> Decrypt(string input, string key, string iv);
    }
}