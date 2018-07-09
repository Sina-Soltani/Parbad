using System;
using System.Globalization;
using System.Reflection;

namespace Parbad.Utilities
{
    internal static class DateTimeManager
    {
        #region Constants

        public const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        public const string LongDateFormat = "dddd, dd MMMM yyyy";

        public const string ShortDateFormat = "yyyy/MM/dd";

        public const string LongDateTimeFormat = "yyyy/MM/dd HH:mm tt";

        public const string ShortDateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        public const string DateTimeFormatWithDay = "dddd, d MMMM ساعت HH:mm";

        public const string DateTimeFormatWithDayAndYear = "dddd, d MMMM yy ساعت HH:mm";

        #endregion

        private static CultureInfo _persianCultureInfo;

        public static CultureInfo GetPersianCultureInfo()
        {
            return _persianCultureInfo ?? (_persianCultureInfo = CreatePersianCultureInfo());
        }

        public static string ToPersianDateTimeString(this DateTime dateTime, string format = ShortDateFormat)
        {
            if (dateTime == DateTime.MinValue)
            {
                throw new ArgumentException("Cannot convert DateTime.MinValue to Persian.", nameof(dateTime));
            }

            return dateTime.ToString(format, GetPersianCultureInfo());
        }

        public static string ToPersianDateTimeString(this DateTime? dateTime, string format = ShortDateFormat)
        {
            if (dateTime == null)
            {
                return string.Empty;
            }

            return dateTime.Value.ToPersianDateTimeString(format);
        }

        public static string ToPersianMonthString(this int month)
        {
            switch (month)
            {
                case 1:
                    return "فروردین";
                case 2:
                    return "اردیبهشت";
                case 3:
                    return "خرداد";
                case 4:
                    return "تیر";
                case 5:
                    return "مرداد";
                case 6:
                    return "شهریور";
                case 7:
                    return "مهر";
                case 8:
                    return "آبان";
                case 9:
                    return "آذر";
                case 10:
                    return "دی";
                case 11:
                    return "بهمن";
                case 12:
                    return "اسفند";
                default:
                    return string.Empty;
            }
        }

        public static PersianDateTime ToPersianDateTime(this DateTime dateTime)
        {
            if (dateTime == null)
            {
                throw new ArgumentNullException(nameof(dateTime));
            }

            string persianDateTimeString = dateTime.ToString("yyyy/MM/dd/HH/mm/ss/fff", GetPersianCultureInfo());

            var array = persianDateTimeString.Split('/');

            return new PersianDateTime(
                int.Parse(array[0]),
                int.Parse(array[1]),
                int.Parse(array[2]),
                int.Parse(array[3]),
                int.Parse(array[4]),
                int.Parse(array[5]),
                int.Parse(array[6]));
        }

        public static PersianDateTime? ToPersianDateTime(this DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }

            string persianDateTimeString = dateTime.Value.ToString("yyyy/MM/dd/HH/mm/ss/fff", GetPersianCultureInfo());

            var array = persianDateTimeString.Split('/');

            return new PersianDateTime(
                int.Parse(array[0]),
                int.Parse(array[1]),
                int.Parse(array[2]),
                int.Parse(array[3]),
                int.Parse(array[4]),
                int.Parse(array[5]),
                int.Parse(array[6]));
        }

        private static CultureInfo CreatePersianCultureInfo()
        {
            var cultureInfo = new CultureInfo("fa-IR");

            DateTimeFormatInfo formatInfo = cultureInfo.DateTimeFormat;

            formatInfo.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };

            formatInfo.DayNames = new[] { "یکشنبه", "دوشنبه", "سه شنبه", "چهار شنبه", "پنجشنبه", "جمعه", "شنبه" };

            var monthNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", ""
            };

            formatInfo.AbbreviatedMonthNames =
                formatInfo.MonthNames =
                    formatInfo.MonthGenitiveNames =
                        formatInfo.AbbreviatedMonthGenitiveNames =
                            monthNames;

            formatInfo.AMDesignator = "ق.ظ";

            formatInfo.PMDesignator = "ب.ظ";

            formatInfo.ShortDatePattern = "yyyy/MM/dd";

            formatInfo.LongDatePattern = "dddd, dd MMMM,yyyy";

            formatInfo.FirstDayOfWeek = DayOfWeek.Saturday;

            var persianCalendar = new PersianCalendar();

            var fieldInfo = cultureInfo.GetType()
                .GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);

            fieldInfo?.SetValue(cultureInfo, persianCalendar);

            var info = formatInfo.GetType()
                .GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);

            info?.SetValue(formatInfo, persianCalendar);

            cultureInfo.NumberFormat.NumberDecimalSeparator = "/";

            cultureInfo.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;

            cultureInfo.NumberFormat.NumberNegativePattern = 0;

            return cultureInfo;
        }
    }
}