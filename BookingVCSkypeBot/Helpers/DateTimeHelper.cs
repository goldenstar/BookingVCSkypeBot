using System;

namespace BookingVCSkypeBot.Helpers
{
    public static class TimeDurationParser
    {
        public static bool TryToHourMinuteParse(this string s, out TimeSpan timeSpan)
        {
            float seconds = 0;
            float current = 0;

            var len = s.Length;
            var isDecimal = false;
            var result = true;

            for (var i = 0; i < len; ++i)
            {
                var c = s[i];

                if (char.IsDigit(c))
                {
                    current = current * 10 + (int)char.GetNumericValue(c);
                }
                else if (c == ',' || c == '.')
                {
                    isDecimal = true;
                }
                else
                {
                    long multiplier = 0;

                    switch (c)
                    {
                        case 'm': multiplier = 60;
                            break;
                        case 'h': multiplier = 3600;
                            break;
                        default:
                        {
                            result = false;
                            break;
                        }
                    }

                    if (isDecimal)
                    {
                        current = current / 10;
                        isDecimal = false;
                    }
                    seconds += current * multiplier;
                    current = 0;
                }
            }

            timeSpan = TimeSpan.FromSeconds(seconds);

            return result;
        }
    }
}