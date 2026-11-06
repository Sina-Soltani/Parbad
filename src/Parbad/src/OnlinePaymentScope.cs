// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Exceptions;

namespace Parbad;

/// <summary>
/// One unit of work with Parbad, e.g. one web request, for applications that do not use dependency injection.
/// It owns the services of that unit of work, such as the storage's database context, and releases them when it
/// is disposed. Create one per operation and dispose it when the operation is finished:
/// <code>
/// using (var parbad = StaticOnlinePayment.CreateScope())
/// {
///     var result = await parbad.OnlinePayment.RequestAsync(invoice);
/// }
/// </code>
/// </summary>
public sealed class OnlinePaymentScope : IDisposable
{
    private readonly IServiceScope _scope;
    private IOnlinePayment? _onlinePayment;

    internal OnlinePaymentScope(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    /// <summary>
    /// The services of this scope.
    /// </summary>
    public IServiceProvider Services => _scope.ServiceProvider;

    /// <summary>
    /// The <see cref="IOnlinePayment"/> of this scope.
    /// </summary>
    /// <exception cref="ParbadServiceNotInitializedException"></exception>
    public IOnlinePayment OnlinePayment =>
        _onlinePayment ??= Services.GetService<IOnlinePayment>() ?? throw new ParbadServiceNotInitializedException();

    /// <summary>
    /// Releases the services of this scope.
    /// </summary>
    public void Dispose() => _scope.Dispose();
}

/// <summary>
/// Extensions for <see cref="IOnlinePaymentAccessor"/>.
/// </summary>
public static class OnlinePaymentAccessorExtensions
{
    /// <summary>
    /// Creates an <see cref="OnlinePaymentScope"/> for one unit of work, e.g. one web request. Dispose it when the
    /// unit of work is finished.
    /// </summary>
    /// <param name="accessor"></param>
    public static OnlinePaymentScope CreateScope(this IOnlinePaymentAccessor accessor)
    {
        if (accessor == null) throw new ArgumentNullException(nameof(accessor));

        return new OnlinePaymentScope(accessor.Services.CreateScope());
    }
}