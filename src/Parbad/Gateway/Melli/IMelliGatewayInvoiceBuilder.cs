using Parbad.Gateway.Melli.Internal.Models;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    /// </summary>
    public interface IMelliGatewayInvoiceBuilder
    {
        /// <summary>
        /// </summary>
        IInvoiceBuilder InvoiceBuilder { get; }

        /// <summary>
        ///     Add share account date to gateway
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        /// <returns></returns>
        IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccount(MultiplexingType type, long amount, int ibanNumber);

        /// <summary>
        ///     Add share account date to gateway
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccounts(MelliMultiplexInvoice account);
    }
}