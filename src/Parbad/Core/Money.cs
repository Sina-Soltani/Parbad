using System;

namespace Parbad.Core
{
    internal struct Money
    {
        private readonly long _value;

        public static readonly Money Zero = 0;

        public Money(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be a negative number.");
            }

            _value = value;
        }

        public bool Equals(Money other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Money money && Equals(money);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public string ToString(string format)
        {
            return _value.ToString(format);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static implicit operator long(Money money)
        {
            return money._value;
        }

        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        public static bool operator ==(Money left, Money right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }

        public static bool operator >(Money left, Money right)
        {
            return left._value > right._value;
        }

        public static bool operator <(Money left, Money right)
        {
            return left._value < right._value;
        }

        public static bool operator >=(Money left, Money right)
        {
            return left._value >= right._value;
        }

        public static bool operator <=(Money left, Money right)
        {
            return left._value <= right._value;
        }

        public static bool TryParse(string value, out Money money)
        {
            try
            {
                money = long.Parse(value);
                return true;
            }
            catch
            {
                money = Zero;
                return false;
            }
        }

        public static Money Parse(string value)
        {
            if (!long.TryParse(value, out var testValue))
            {
                throw new Exception($"Cannot parse {value} to Money.");
            }

            return testValue;
        }
    }
}