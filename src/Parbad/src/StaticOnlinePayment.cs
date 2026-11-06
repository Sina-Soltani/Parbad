// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad;

/// <summary>
/// Provides a static version of <see cref="IOnlinePayment"/>.
/// <para> You use this method when using no dependency injections such as
/// Microsoft.Extensions.DependencyInjection in your application,
/// otherwise you don't need to use this and can inject the <see cref="IOnlinePayment"/>
/// interface wherever you need.
/// </para>
/// <para>Create one <see cref="OnlinePaymentScope"/> per operation (e.g. per web request) with
/// <see cref="CreateScope"/>, and dispose it when the operation is finished.</para>
/// </summary>
public static class StaticOnlinePayment
{
    private static IOnlinePaymentAccessor _parbadAccessor;

    /// <summary>
    /// Creates an <see cref="OnlinePaymentScope"/> for one operation, e.g. one web request. It owns the services of
    /// that operation, such as the storage's database context. Dispose it when the operation is finished:
    /// <code>
    /// using (var parbad = StaticOnlinePayment.CreateScope())
    /// {
    ///     var result = await parbad.OnlinePayment.RequestAsync(invoice);
    /// }
    /// </code>
    /// </summary>
    /// <exception cref="InvalidOperationException">Parbad is not built yet (<c>ParbadBuilder.Build()</c>).</exception>
    public static OnlinePaymentScope CreateScope()
    {
        if (_parbadAccessor == null) throw new InvalidOperationException($"{nameof(StaticOnlinePayment)} is not initialized.");

        return _parbadAccessor.CreateScope();
    }

    /// <summary>
    /// Initializes the <see cref="StaticOnlinePayment"/> class for static usage.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    internal static void Initialize(IOnlinePaymentAccessor parbadAccessor)
    {
        _parbadAccessor = parbadAccessor ?? throw new ArgumentNullException(nameof(parbadAccessor));
    }
}