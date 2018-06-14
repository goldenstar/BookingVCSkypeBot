using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace BookingVCSkypeBot.Authentication
{
    public static class Extensions
    {
        public static void StoreAuthResult(this IBotContext context, AuthResult authResult, IAuthProvider authProvider)
        {
            context.UserData.SetValue($"{ContextKey.AuthResult}", authResult);
        }
    }
}
