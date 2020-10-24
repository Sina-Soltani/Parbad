using System;
using System.Collections.Generic;
using Parbad.Properties;

namespace Parbad.Gateway.Melli.Internal.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MultiplexingAccount
    {
        /// <summary>
        /// Type of sharing
        /// </summary>
        public MultiplexingType? Type { get; set; }
        /// <summary>
        /// the information of account top to 10
        /// </summary>
        public List<MultiplexingDataItem> MultiplexingRows { get; set; }
    }
    /// <summary>
    /// type of share
    /// </summary>
    public enum MultiplexingType
    {
        /// <summary>
        /// Percentage share
        /// </summary>
        Percentage,
        /// <summary>
        /// Amount type
        /// </summary>
        Amount
    }
    /// <summary>
    /// share data
    /// </summary>
    public class MultiplexingDataItem
    {
        /// <summary>
        /// fill share data
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="ibanNumber"></param>
        public MultiplexingDataItem(Money amount, int ibanNumber)
        {

            if (ibanNumber < 0) throw new ArgumentOutOfRangeException(nameof(ibanNumber), string.Format(Resources.MultiplexingDataItem_MultiplexingDataItem__0__cannot_be_a_negative_number, nameof(ibanNumber)));
            Value = amount;
            IbanNumber = ibanNumber;
        }
        /// <summary>
        /// Iban number register in bank
        /// </summary>
        public int IbanNumber { get; set; }
        /// <summary>
        /// value of share
        /// </summary>
        public long Value { get; set; }
    }
}
