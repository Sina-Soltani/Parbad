// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Parbad
{
    /// <summary>
    /// Defines money unit.
    /// <para>
    /// Note: The official unit of currency in Iran is the Iranian rial (IR).
    /// It means the amount of the invoice will be sent to Iranian gateways automatically
    /// as <see cref="Int64"/> by Parbad.
    /// </para>
    /// <para>Examples:
    /// <para>decimal a = Money</para>
    /// <para>long a = Money</para>
    /// </para>
    /// </summary>
    public readonly struct Money : IComparable<Money>
    {
        /// <summary>
        /// Defines money unit.
        /// <para>
        /// Note: The official unit of currency in Iran is the Iranian rial (IR).
        /// It means the amount of the invoice will be sent to Iranian gateways automatically
        /// as <see cref="Int64"/> by Parbad.
        /// </para>
        /// </summary>
        /// <param name="value">The amount of money.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Money(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; }

        public Money AddAmount(decimal amount)
        {
            return new Money(Value + amount);
        }

        public bool Equals(Money other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Money other && Equals(other);
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

        /// <exception cref="Exception"></exception>
        public static Money Parse(string amount)
        {
            if (!decimal.TryParse(amount, out var testValue))
            {
                throw new Exception($"Cannot parse {amount} to Money.");
            }

            return testValue;
        }

        public static bool TryParse(string value, out Money money)
        {
            try
            {
                money = Parse(value);
                return true;
            }
            catch
            {
                money = default;
                return false;
            }
        }

        public static implicit operator decimal(Money money)
        {
            return money.Value;
        }

        public static implicit operator long(Money money)
        {
            return (long)money.Value;
        }

        public static implicit operator Money(decimal amount) => Parse(amount);

        public static implicit operator Money(long amount) => Parse(amount);

        public static bool operator >(Money left, Money right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(Money left, Money right)
        {
            return !(left > right);
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
}
