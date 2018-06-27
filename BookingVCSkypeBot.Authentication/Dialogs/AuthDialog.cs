using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingVCSkypeBot.Authentication.Helpers;
using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Authentication.Dialogs
{
    [Serializable]
    public class AuthDialog : IDialog<AuthResult>
    {
        private readonly IAuthProvider authProvider;
        private readonly AuthenticationOptions authOptions;

        public AuthDialog(IAuthProvider authProvider, AuthenticationOptions authOptions)
        {
            this.authProvider = authProvider;
            this.authOptions = authOptions;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;

            if (context.UserData.TryGetValue($"{ContextKey.AuthResult}", out AuthResult authResult))
            {
                await CheckAuthUser(context, msg, authResult);
            }
            else
            {
                await TrySetUser(context, msg);
            }
        }

        private async Task CheckAuthUser(IDialogContext context, IMessageActivity msg, AuthResult authResult)
        {
            try
            {
                context.UserData.TryGetValue($"{ContextKey.MagicNumberValidated}", out bool magicNumberValidated);

                if (magicNumberValidated)
                {
                    await TrySetUser(context, msg);
                }
                else
                {
                    await GetMagicNumber(context, msg, authResult);
                }
            }
            catch
            {
                ClearUserData(context.UserData);

                await context.PostAsync(AuthRes.WrongAuthenticating);

                context.Done<AuthResult>(null);
            }
        }

        private async Task TrySetUser(IDialogContext context, IMessageActivity msg)
        {
            var token = await authProvider.GetTokenByContextAsync(authOptions, context);

            if (token != null)
            {
                context.Done(token);
            }
            else
            {
                await LoginUser(context, msg);
            }
        }

        private async Task GetMagicNumber(IDialogContext context, IMessageActivity msg, AuthResult authResult)
        {
            if (context.UserData.TryGetValue($"{ContextKey.MagicNumber}", out int magicNumber))
            {
                if (msg.Text == null)
                {
                    await context.PostAsync(AuthRes.EnterMagicNumber);

                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    if (magicNumber.ToString() == msg.Text)
                    {
                        context.UserData.SetValue($"{ContextKey.MagicNumberValidated}", true);

                        await context.PostAsync(string.Format(AuthRes.LoggedIn, authResult.UserName));

                        context.Done(authResult);
                    }
                    else
                    {
                        ClearUserData(context.UserData);

                        await context.PostAsync(AuthRes.WrongMagicNumber);

                        context.Wait(MessageReceivedAsync);
                    }
                }
            }
        }

        private static void ClearUserData(IBotDataBag userData)
        {
            userData.RemoveValue($"{ContextKey.AuthResult}");
            userData.SetValue($"{ContextKey.MagicNumberValidated}", false);
            userData.RemoveValue($"{ContextKey.MagicNumber}");
        }

        private async Task LoginUser(IDialogContext context, IActivity msg)
        {
            context.UserData.SetValue($"{ContextKey.AuthOptions}", authOptions);

            await PromptToLogin(context, msg);

            context.Wait(MessageReceivedAsync);
        }

        private async Task<Task> PromptToLogin(IDialogContext context, IActivity msg)
        {
            var response = context.MakeMessage();

            response.Recipient = msg.From;

            var loginCard = new LoginCard(authProvider, authOptions);

            var card = await loginCard.GetSinginCard(context);

            response.Attachments = new List<Attachment>
            {
                card.ToAttachment()
            };

            return context.PostAsync(response);
        }
    }
}