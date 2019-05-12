// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Parbad.Properties;

namespace Parbad
{
    /// <summary>
    /// Defines money unit.
    /// <para>
    /// Note: The official unit of currency in Iran is the Iranian rial (IR).
    /// It means the amount of the invoice will be sent to Iranian gateways automatically
    /// as <see cref="Int64"/> by Parbad.
    /// </para>
    /// </summary>
    public class Money : IComparable<Money>
    {
        private readonly decimal _amount;

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
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), Resources.AmountCannotBeNegative);

            _amount = amount;
        }

        public bool Equals(Money other)
        {
            return other != null && _amount == other._amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _amount.GetHashCode();
        }

        public int CompareTo(Money other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return _amount.CompareTo(other._amount);
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider format)
        {
            return _amount.ToString(format);
        }

        public string ToString(string format)
        {
            return _amount.ToString(format);
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
                money = null;
                return false;
            }
        }

        public static implicit operator decimal(Money money)
        {
            if (money == null) throw new ArgumentNullException(nameof(money));

            return money._amount;
        }

        public static implicit operator long(Money money)
        {
            if (money == null) throw new ArgumentNullException(nameof(money));

            return (long)money._amount;
        }

        public static implicit operator Money(decimal amount) => Parse(amount);

        public static implicit operator Money(long amount) => Parse(amount);

        public static bool operator >(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left._amount > right._amount;
        }

        public static bool operator <(Money left, Money right)
        {
            return !(left > right);
        }

        public static bool operator >=(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left._amount >= right._amount;
        }

        public static bool operator <=(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return left._amount <= right._amount;
        }

        public static Money operator +(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left._amount + right._amount);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left._amount - right._amount);
        }

        public static Money operator *(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left._amount * right._amount);
        }

        public static Money operator /(Money left, Money right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return new Money(left._amount / right._amount);
        }
    }
}
