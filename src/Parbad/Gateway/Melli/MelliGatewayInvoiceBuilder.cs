using Parbad.Gateway.Melli.Internal;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using Parbad.Properties;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    /// 
    /// </summary>
    public class MelliGatewayInvoiceBuilder : IMelliGatewayInvoiceBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        public IInvoiceBuilder InvoiceBuilder { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceBuilder"></param>
        public MelliGatewayInvoiceBuilder(IInvoiceBuilder invoiceBuilder)
        {
            InvoiceBuilder = invoiceBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        /// <returns></returns>
        public IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccount(MultiplexingType type, long amount, int ibanNumber)
        {
            return AddMelliMultiplexingAccounts(new MultiplexingAccount
            {
                Type = type,
                MultiplexingRows = new List<MultiplexingDataItem>
                {
                    new MultiplexingDataItem(amount,ibanNumber)
                }
            });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IMelliGatewayInvoiceBuilder AddMelliMultiplexingAccounts(MultiplexingAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (account.MultiplexingRows.Count == 0) throw new ArgumentException(Resources.MelliGatewayInvoiceBuilder_AddMelliMultiplexingAccounts_Accounts_cannot_be_an_empty_collection_, nameof(account));

            IList<MultiplexingDataItem> existingAccounts = new List<MultiplexingDataItem>();

            foreach (var item in account.MultiplexingRows)
            {
                existingAccounts.Add(item);
            }

            InvoiceBuilder.AddAdditionalData(MelliHelper.MultiplexingAccountsKey, new MultiplexingAccount
            {
                Type = account.Type,
                MultiplexingRows = existingAccounts.Select(x=> new MultiplexingDataItem(x.Value,x.IbanNumber)).ToList()
            });

            return this;
        }


    }
}
