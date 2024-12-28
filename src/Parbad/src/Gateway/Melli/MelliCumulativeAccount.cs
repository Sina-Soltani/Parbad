using System.Collections.Generic;

namespace Parbad.Gateway.Melli;

public class MelliCumulativeAccount
{
    public enum Types
    {
        /// <summary>
        /// تسهیم با ارسال مبلغ
        /// </summary>
        Amount,
        
        /// <summary>
        /// تسهیم با ارسال درصد
        /// </summary>
        Percentage
    }

    public class MultiplexingRow
    {
        /// <summary>
        /// ردیف یا شماره شبای حساب
        /// </summary>
        public string IbanNumber { get; set; }
        
        /// <summary>
        /// مبلغ یا درصد
        /// </summary>
        public int Value { get; set; }
    }
    
    /// <summary>
    /// نوع تسهیم
    /// </summary>
    public Types Type { get; set; }
    
    /// <summary>
    /// مقدار تسهیم
    /// </summary>
    public List<MultiplexingRow> MultiplexingRows { get; set; }
}