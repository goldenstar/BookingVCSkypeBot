using System.Collections.Generic;
using System.Linq;

namespace BookingVCSkypeBot.Authentication.Models
{
    public class CancellationWords
    {
        public static List<string> GetCancellationWords()
        {
            return AuthText.CancellationWords.Split(',').ToList();
        }
    }
}