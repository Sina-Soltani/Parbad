using System;
using System.Collections.Generic;
using Parbad.Properties;

namespace Parbad.Gateway.Melli.Internal.Models
{
    /// <summary>
    /// Main class that hold the share account info
    /// </summary>
    public class MelliMultiplexInvoice
    {
        /// <summary>
        ///     Type of sharing
        /// </summary>
        public MultiplexingType? Type { get; set; }

        /// <summary>
        ///     The information of account top to 10
        /// </summary>
        public List<MelliMultiplexInvoiceItem> MultiplexingRows { get; set; }
    }

    /// <summary>
    ///     Type of share
    /// </summary>
    public enum MultiplexingType
    {
        /// <summary>
        ///     Percentage share
        /// </summary>
        Percentage,

        /// <summary>
        ///     Amount type
        /// </summary>
        Amount
    }

    /// <summary>
    ///     Share data
    /// </summary>
    public class MelliMultiplexInvoiceItem
    {
        /// <summary>
        ///     Fill share data
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        /// <param name="type"></param>
        public MelliMultiplexInvoiceItem(long amount, int ibanNumber, MultiplexingType type)
        {
            if (ibanNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(ibanNumber),
                    string.Format(Resources.Cannot_be_a_negative_number,
                        nameof(ibanNumber)));
            switch (type)
            {
                // Check while the amount is in number do not less or equal to zero
                case MultiplexingType.Amount when amount <= 0:
                    throw new ArgumentOutOfRangeException(nameof(amount),
                        string.Format(Resources.Cannot_be_a_negative_number,
                            nameof(amount)));

                //If amount is percent type do not less or equal to zero
                case MultiplexingType.Percentage when amount <= 0:
                    throw new ArgumentOutOfRangeException(nameof(amount),
                        string.Format(Resources.Cannot_be_a_negative_number,
                            nameof(amount)));

                //If amount is percent type do not greater than 100
                case MultiplexingType.Percentage when amount > 100:
                    throw new ArgumentOutOfRangeException(nameof(amount),
                        string.Format(Resources.Percentage_cannot_be_greater_than_hundred,
                            nameof(amount)));
                default:
                    Value = amount;
                    IbanNumber = ibanNumber;
                    break;
            }
        }

        /// <summary>
        ///     Iban number register in bank
        /// </summary>
        public int IbanNumber { get; }

        /// <summary>
        ///     Value of share
        /// </summary>
        public long Value { get; }
    }
}