using System;

namespace BookingVCSkypeBot.Helper
{
    public static class TimeDurationParser
    {
        public static TimeSpan TryHMParse(this string s, out TimeSpan result)
        {
            float seconds = 0;
            float current = 0;

            var len = s.Length;
            var isDecimal = false;
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
                    long multiplier;

                    switch (c)
                    {
                        case 's': multiplier = 1; break;      // seconds
                        case 'm': multiplier = 60; break;     // minutes
                        case 'h': multiplier = 3600; break;   // hours
                        case 'd': multiplier = 86400; break;  // days
                        case 'w': multiplier = 604800; break; // weeks
                        default:
                            throw new FormatException(
                                $"'{s}': Invalid duration character {c} at position {i}. Supported characters are h and m");
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

            if (current != 0)
            {
                throw new FormatException($"'{s}': missing duration specifier in the end of the string. Supported characters are h and m");
            }

            result = TimeSpan.FromSeconds(seconds);

            return result;
        }
    }
}