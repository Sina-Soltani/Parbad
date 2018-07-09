namespace Parbad.Utilities
{
    internal struct PersianDateTime
    {
        public PersianDateTime(int year, int month, int day) : this(year, month, day, 0, 0, 0)
        {
        }

        public PersianDateTime(int year, int month, int day, int hour, int minute, int second) : this(year, month, day, hour, minute, second, 0)
        {
        }

        public PersianDateTime(int year, int month, int day, int hour, int minute, int second, int miliSecond)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            MiliSecond = miliSecond;
        }

        public int Year { get; }

        public int Month { get; }

        public int Day { get; }

        public int Hour { get; }

        public int Minute { get; }

        public int Second { get; }

        public int MiliSecond { get; }

        public string ToString(string format)
        {
            //  Replace year
            format = format.Replace("yyyy", "{0}");

            //  Replace month
            format = format.Replace("MM", "{1}");

            //  Replace day
            format = format.Replace("dd", "{2}");

            //  Replace hour
            format = format.Replace("HH", "{3}");

            //  Replace minute
            format = format.Replace("mm", "{4}");

            //  Replace second
            format = format.Replace("ss", "{5}");

            //  Replace miliSecond
            format = format.Replace("fff", "{6}");

            return string.Format(
                format,
                Year.ToString().PadLeft(4, '0'),
                Month.ToString().PadLeft(2, '0'),
                Day.ToString().PadLeft(2, '0'),
                Hour.ToString().PadLeft(2, '0'),
                Minute.ToString().PadLeft(2, '0'),
                Second.ToString().PadLeft(2, '0'),
                MiliSecond.ToString().PadLeft(3, '0'));
        }
    }
}