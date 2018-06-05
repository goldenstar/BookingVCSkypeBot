using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BookingVCSkypeBot.Authentication.Dialogs
{
    [Serializable]
    public class AuthDialog : IDialog<AuthResult>
    {
        protected IAuthProvider AuthProvider;
        protected AuthenticationOptions AuthOptions;
        protected string Prompt { get; }

        public AuthDialog(IAuthProvider authProvider, AuthenticationOptions authOptions, string prompt = "Please click to sign in: ")
        {
            Prompt = prompt;
            AuthProvider = authProvider;
            AuthOptions = authOptions;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;

            if (context.UserData.TryGetValue($"{AuthProvider.Name}{ContextConstants.AuthResultKey}", out AuthResult authResult))
            {
                try
                {
                    //IMPORTANT: DO NOT REMOVE THE MAGIC NUMBER CHECK THAT WE DO HERE. THIS IS AN ABSOLUTE SECURITY REQUIREMENT
                    //REMOVING THIS WILL REMOVE YOUR BOT AND YOUR USERS TO SECURITY VULNERABILITIES. 
                    //MAKE SURE YOU UNDERSTAND THE ATTACK VECTORS AND WHY THIS IS IN PLACE.
                    context.UserData.TryGetValue<string>($"{AuthProvider.Name}{ContextConstants.MagicNumberValidated}", out var validated);
                    if (validated == "true" || !AuthOptions.UseMagicNumber)
                    {
                        // Try to get token to ensure it is still good
                        var token = await AuthProvider.GetAccessToken(AuthOptions, context);
                        if (token != null)
                        {
                            context.Done(token);
                        }
                        else
                        {
                            // Save authenticationOptions in UserData
                            context.UserData.SetValue($"{AuthProvider.Name}{ContextConstants.AuthOptions}", AuthOptions);

                            // Get ConversationReference and combine with AuthProvider type for the callback
                            var conversationRef = context.Activity.ToConversationReference();
                            var state = GetStateParam(conversationRef);
                            var authenticationUrl = await AuthProvider.GetAuthUrlAsync(AuthOptions, state);
                            await PromptToLogin(context, msg, authenticationUrl);
                            context.Wait(MessageReceivedAsync);
                        }
                    }
                    else
                    {
                        if (context.UserData.TryGetValue<int>($"{AuthProvider.Name}{ContextConstants.MagicNumberKey}", out var magicNumber))
                        {
                            if (msg.Text == null)
                            {
                                await context.PostAsync("Please paste back the number you received in your authentication screen.");

                                context.Wait(MessageReceivedAsync);
                            }
                            else
                            {
                                // handle at mentions in Teams
                                var text = msg.Text;
                                if (text.Contains("</at>"))
                                {
                                    text = text.Substring(text.IndexOf("</at>") + 5).Trim();
                                }

                                if (text.Length >= 6 && magicNumber.ToString() == text.Substring(0, 6))
                                {
                                    context.UserData.SetValue($"{AuthProvider.Name}{ContextConstants.MagicNumberValidated}", "true");
                                    await context.PostAsync($"Thanks {authResult.UserName}. You are now logged in. ");
                                    context.Done(authResult);
                                }
                                else
                                {
                                    context.UserData.RemoveValue($"{AuthProvider.Name}{ContextConstants.AuthResultKey}");
                                    context.UserData.SetValue($"{AuthProvider.Name}{ContextConstants.MagicNumberValidated}", "false");
                                    context.UserData.RemoveValue($"{AuthProvider.Name}{ContextConstants.MagicNumberKey}");
                                    await context.PostAsync("I'm sorry but I couldn't validate your number. Please try authenticating once again. ");
                                    context.Wait(MessageReceivedAsync);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    context.UserData.RemoveValue($"{AuthProvider.Name}{ContextConstants.AuthResultKey}");
                    context.UserData.SetValue($"{AuthProvider.Name}{ContextConstants.MagicNumberValidated}", "false");
                    context.UserData.RemoveValue($"{AuthProvider.Name}{ContextConstants.MagicNumberKey}");
                    await context.PostAsync("I'm sorry but something went wrong while authenticating.");
                    context.Done<AuthResult>(null);
                }
            }
            else
            {
                // Try to get token
                var token = await AuthProvider.GetAccessToken(AuthOptions, context);
                if (token != null)
                {
                    context.Done(token);
                }
                else
                {
                    if (msg.Text != null && CancellationWords.GetCancellationWords().Contains(msg.Text.ToUpper()))
                    {
                        context.Done<AuthResult>(null);
                    }
                    else
                    {
                        // Save authenticationOptions in UserData
                        context.UserData.SetValue($"{AuthProvider.Name}{ContextConstants.AuthOptions}", AuthOptions);

                        // Get ConversationReference and combine with AuthProvider type for the callback
                        var conversationRef = context.Activity.ToConversationReference();
                        var state = GetStateParam(conversationRef);
                        var authenticationUrl = await AuthProvider.GetAuthUrlAsync(AuthOptions, state);
                        await PromptToLogin(context, msg, authenticationUrl);
                        context.Wait(MessageReceivedAsync);
                    }
                }
            }
        }

        private string GetStateParam(ConversationReference conversationRef)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["conversationRef"] = UrlToken.Encode(conversationRef);
            queryString["providerassembly"] = AuthProvider.GetType().Assembly.FullName;
            queryString["providertype"] = AuthProvider.GetType().FullName;
            queryString["providername"] = AuthProvider.Name;
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(queryString.ToString()));
        }

        /// <summary>
        /// Prompts the user to login. This can be overridden inorder to allow custom prompt messages or cards per channel.
        /// </summary>
        /// <param name="context">Chat context</param>
        /// <param name="msg">Chat message</param>
        /// <param name="authenticationUrl">OAuth URL for authenticating user</param>
        /// <returns>Task from Posting or prompt to the context.</returns>
        protected virtual Task PromptToLogin(IDialogContext context, IMessageActivity msg, string authenticationUrl)
        {
            var plCard = msg.ChannelId == "msteams"
                ? new SigninCard(Prompt, GetCardActions(authenticationUrl, "openUrl"))
                : new SigninCard(Prompt, GetCardActions(authenticationUrl, "signin"));

            var plAttachment = plCard.ToAttachment();

            var response = context.MakeMessage();
            response.Recipient = msg.From;
            response.Type = "message";

            response.Attachments = new List<Attachment>
            {
                plAttachment
            };

            return context.PostAsync(response);
        }

        private static List<CardAction> GetCardActions(string authenticationUrl, string actionType)
        {
            var cardButtons = new List<CardAction>();
            var plButton = new CardAction
            {
                Value = authenticationUrl,
                Type = actionType,
                Title = "Authentication Required"
            };
            cardButtons.Add(plButton);
            return cardButtons;
        }
    }
}