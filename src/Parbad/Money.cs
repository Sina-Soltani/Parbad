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
        /// <param name="amount">The amount of money.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Money(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }

        public Money AddAmount(decimal amount)
        {
            return new Money(Amount + amount);
        }

        public bool Equals(Money other)
        {
            return other != null && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode();
        }

        public int CompareTo(Money other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return Amount.CompareTo(other.Amount);
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider format)
        {
            return Amount.ToString(format);
        }

        public string ToString(string format)
        {
            return Amount.ToString(format);
        }

        public static Money Parse(decimal amount) => new Money(amount);

        public static Money Parse(long amount) => new Money(amount);

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
            if (money == null) throw new ArgumentNullException(nameof(money));

            return money.Amount;
        }

        public static implicit operator long(Money money)
        {
            if (money == null) throw new ArgumentNullException(nameof(money));

            return (long)money.Amount;
        }

        public static implicit operator Money(decimal amount) => Parse(amount);

        public static implicit operator Money(long amount) => Parse(amount);

        public static bool operator >(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            return !(left > right);
        }

        public static bool operator >=(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left.Amount >= right.Amount;
        }

        public static bool operator <=(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left.Amount <= right.Amount;
        }

        public static Money operator +(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left.Amount + right.Amount);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left.Amount - right.Amount);
        }

        public static Money operator *(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left.Amount * right.Amount);
        }

        public static Money operator /(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left.Amount / right.Amount);
        }
    }
}
