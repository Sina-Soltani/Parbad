using System;
using System.Collections.Generic;
using System.Linq;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.InvoiceBuilder;
using Parbad.Properties;

namespace Parbad.Gateway.Melli.Internal
{
    /// <summary>
    /// </summary>
    internal class MelliGatewayInvoiceBuilder : IMelliGatewayInvoiceBuilder
    {
        /// <summary>
        /// </summary>
        /// <param name="invoiceBuilder"></param>
        public MelliGatewayInvoiceBuilder(IInvoiceBuilder invoiceBuilder)
        {
            InvoiceBuilder = invoiceBuilder;
        }

        /// <summary>
        /// </summary>
        public IInvoiceBuilder InvoiceBuilder { get; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        /// <returns></returns>
        public IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccount(MultiplexingType type, long amount,
            int ibanNumber)
        {
            return AddMelliMultiplexingAccounts(new MelliMultiplexInvoice
            {
                Type = type,
                MultiplexingRows = new List<MelliMultiplexInvoiceItem>
                {
                    new MelliMultiplexInvoiceItem(amount, ibanNumber, type)
                }
            });
        }


        /// <summary>
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccounts(MelliMultiplexInvoice account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (account.MultiplexingRows.Count == 0)
                throw new ArgumentException(
                    Resources
                        .MelliGatewayInvoiceBuilder_AddMelliMultiplexingAccounts_Accounts_cannot_be_an_empty_collection_,
                    nameof(account));

            IList<MelliMultiplexInvoiceItem> existingAccounts = new List<MelliMultiplexInvoiceItem>();

            foreach (var item in account.MultiplexingRows) existingAccounts.Add(item);

            InvoiceBuilder.AddAdditionalData(MelliHelper.MultiplexingAccountsKey, new MelliMultiplexInvoice
            {
                Type = account.Type,
                MultiplexingRows = existingAccounts.Select(x => new MelliMultiplexInvoiceItem(x.Value, x.IbanNumber, account.Type ?? MultiplexingType.Amount))
                    .ToList()
            });

            return this;
        }
    }
}