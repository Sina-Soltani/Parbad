using Microsoft.AspNetCore.Http;
using System;

namespace Parbad.Options;

/// <summary>
/// Provides configuration for Parbad.
/// </summary>
public class ParbadOptions
{
    /// <summary>
    /// Enables or disables the logging. The default value is true.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Contains all messages that Parbad uses in results.
    /// </summary>
    public MessagesOptions Messages { get; set; } = new MessagesOptions();

    /// <summary>
    /// A factory to represent Nonce. The value will be used in inline-scripts and CSS links
    /// to support CSP (Content-Security-Policy). If your application doesn't include any CSP, you can leave this property null. 
    /// </summary>
    public Func<HttpContext, string>? NonceFactory { get; set; }

    /// <summary>
    /// The timeout of each HTTP call Parbad makes to a gateway. The default value is 30 seconds, instead of the
    /// 100 seconds of <see cref="System.Net.Http.HttpClient"/>. Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>
    /// for no timeout.
    /// <para>
    /// A verification that times out throws and leaves the payment open, so it can be verified again. A timeout
    /// set for one gateway with <c>WithHttpClient</c> takes precedence.
    /// </para>
    /// </summary>
    public TimeSpan GatewayTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
