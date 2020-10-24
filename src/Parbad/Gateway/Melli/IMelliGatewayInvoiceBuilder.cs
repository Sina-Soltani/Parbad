using Parbad.Gateway.Melli.Internal.Models;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMelliGatewayInvoiceBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        IInvoiceBuilder InvoiceBuilder { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        /// <returns></returns>
        IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccount(MultiplexingType type, long amount, int ibanNumber);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccounts(MultiplexingAccount account);
    }
}
