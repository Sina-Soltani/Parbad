// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Parbad;

/// <summary>
/// An amount of money.
/// <para>
/// Parbad never converts the amount. It is sent to the gateway exactly as given, as a whole number
/// (<see cref="Int64"/>). Set it in the unit the gateway expects: most Iranian gateways expect Iranian rials (IRR),
/// but some payment providers work in tomans. Check the gateway's documentation.
/// </para>
/// <para>
/// Converting to <see cref="long"/> is explicit and throws <see cref="InvalidOperationException"/> when the amount
/// has a fraction, so no part of an amount is dropped silently. Equality compares the exact values.
/// </para>
/// <para>Examples:
/// <para>Money m = 1000;</para>
/// <para>decimal d = m;</para>
/// <para>long l = (long)m;</para>
/// </para>
/// </summary>
public readonly struct Money : IComparable<Money>, IEquatable<Money>, IEquatable<decimal>, IEquatable<long>
{
    /// <summary>
    /// Initializes an amount of money.
    /// <para>
    /// Parbad never converts the amount. Set it in the unit the gateway expects (see <see cref="Money"/>).
    /// </para>
    /// </summary>
    /// <param name="value">The amount of money.</param>
    public Money(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public Money AddAmount(decimal amount)
    {
        return new Money(Value + amount);
    }

    public bool Equals(Money? other) => other.HasValue && Equals(other.Value);
    public bool Equals(Money other) => Value == other.Value;

    public bool Equals(long? other) => other.HasValue && Equals(other.Value);
    public bool Equals(long other) => Value == other;

    public bool Equals(decimal? other) => other.HasValue && Equals(other.Value);
    public bool Equals(decimal other) => Value == other;

    public override bool Equals(object obj)
    {
        var equals =
            (obj is Money other && Equals(other)) ||
            (obj is int intNumber) && Equals((long)intNumber) ||
            (obj is long longNumber) && Equals(longNumber) ||
            (obj is decimal decimalNumber) && Equals(decimalNumber);

        return equals;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public int CompareTo(Money other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return ToString(CultureInfo.InvariantCulture);
    }

    public string ToString(IFormatProvider format)
    {
        return Value.ToString(format);
    }

    public string ToString(string format)
    {
        return Value.ToString(format);
    }

    public static Money Parse(decimal amount) => new Money(amount);

    public static Money Parse(long amount) => new Money(amount);

    /// <summary>
    /// Parses an amount written with the invariant culture (e.g. "1000" or "1000.5"), whatever the server's culture.
    /// </summary>
    /// <exception cref="FormatException"><paramref name="amount"/> is not a number.</exception>
    public static Money Parse(string amount)
    {
        if (!TryParse(amount, out var money))
        {
            throw new FormatException($"Cannot parse '{amount}' to Money.");
        }

        return money;
    }

    /// <summary>
    /// Parses an amount written with the invariant culture (e.g. "1000" or "1000.5"), whatever the server's culture.
    /// </summary>
    public static bool TryParse(string value, out Money money)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            money = new Money(amount);
            return true;
        }

        money = default;
        return false;
    }

    public static implicit operator decimal(Money money)
    {
        return money.Value;
    }

    /// <summary>
    /// Converts the amount to a whole number.
    /// </summary>
    /// <exception cref="InvalidOperationException">The amount has a fraction.</exception>
    /// <exception cref="OverflowException">The amount does not fit into a <see cref="long"/>.</exception>
    public static explicit operator long(Money money)
    {
        if (decimal.Truncate(money.Value) != money.Value)
        {
            throw new InvalidOperationException($"The amount {money} has a fraction and cannot be converted to a whole number.");
        }

        return decimal.ToInt64(money.Value);
    }

    public static implicit operator Money(decimal amount) => Parse(amount);

    public static implicit operator Money(long amount) => Parse(amount);

    public static bool operator >(Money left, Money right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <(Money left, Money right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >=(Money left, Money right)
    {
        return left.Value >= right.Value;
    }

    public static bool operator <=(Money left, Money right)
    {
        return left.Value <= right.Value;
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Value + right.Value);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Value - right.Value);
    }

    public static Money operator *(Money left, Money right)
    {
        return new Money(left.Value * right.Value);
    }

    public static Money operator /(Money left, Money right)
    {
        return new Money(left.Value / right.Value);
    }
}