using Parbad.Exceptions;

namespace Parbad.Abstraction
{
    /// <summary>
    /// Describes a gateway account.
    /// </summary>
    public abstract class GatewayAccount
    {
        /// <summary>
        /// Gets or sets the name of this account. The default value is "Default".
        /// <para>Note: Make sure that accounts have different names. Otherwise a <see cref="DuplicateAccountException"/> will throw.</para>
        /// </summary>
        public string Name { get; set; } = "Default";
    }
}
