namespace Parbad.Utilities
{
    internal static class Extensions
    {
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public static string ToggleStringAtStart(this string value, string stringValue, bool shouldHave)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (shouldHave)
            {
                if (!value.StartsWith(stringValue))
                {
                    value = stringValue + value;
                }
            }
            else
            {
                if (value.StartsWith(stringValue))
                {

                    value = value.Substring(stringValue.Length, value.Length - stringValue.Length);
                }
            }

            return value;
        }

        public static string ToggleStringAtEnd(this string value, string stringValue, bool shouldHave)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (shouldHave)
            {
                if (!value.EndsWith(stringValue))
                {
                    value += stringValue;
                }
            }
            else
            {
                if (value.EndsWith(stringValue))
                {
                    value = value.Substring(0, value.Length - stringValue.Length);
                }
            }

            return value;
        }
    }
}