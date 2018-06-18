using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using BookingVCSkypeBot.Authentication;
using BookingVCSkypeBot.Authentication.Dialogs;
using BookingVCSkypeBot.Authentication.Models;
using BookingVCSkypeBot.Core.Interfaces;
using BookingVCSkypeBot.Dialogs.MeetingDateTimeDialogs;
using BookingVCSkypeBot.Models;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly ILocationService locationService;
        private readonly IAuthProvider authProvider;

        private readonly MeetingModel meetingModel;

        public RootDialog(ILocationService locationService, IAuthProvider authProvider)
        {
            this.locationService = locationService;
            this.authProvider = authProvider;

            meetingModel = new MeetingModel();
        }

        public async Task StartAsync(IDialogContext context)
        {
            

            await context.PostAsync("Hi, I'm the Basic Multi Dialog bot. Let's get started.");

            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            var options = new AuthenticationOptions
            {
                Authority = ConfigurationManager.AppSettings["aad:Authority"],
                ClientId = ConfigurationManager.AppSettings["aad:ClientIdTest"],
                ClientSecret = ConfigurationManager.AppSettings["aad:ClientSecretTest"],
                RedirectUrl = ConfigurationManager.AppSettings["aad:Callback"],
                Scopes = new[] { ConfigurationManager.AppSettings["aad:Scopes"] }
            };

            var authDialog = new AuthDialog(authProvider, options);

            await context.Forward(authDialog, AuthenticationResumeAfterAsync, message, CancellationToken.None);
        }

        private async Task AuthenticationResumeAfterAsync(IDialogContext context, IAwaitable<AuthResult> result)
        {
            meetingModel.AuthResult = await result;

            SelectLocation(context);
        }

        private void SelectLocation(IDialogContext context)
        {
            var locations = locationService.ListAll().Select(x => x.Name).ToList().AsReadOnly();

            var options = new PromptOptions<string>(DialogRes.Location, tooManyAttempts: DialogRes.ManyAttempts,
                options: locations, attempts: 2);

            PromptDialog.Choice(context, SelectLocationResumeAfterAsync, options);
        }

        private async Task SelectLocationResumeAfterAsync(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                meetingModel.LocationName = await result;

                context.Call(new PeopleCountDialog(), PeopleCountResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                SelectLocation(context);
            }
        }

        private async Task PeopleCountResumeAfterAsync(IDialogContext context, IAwaitable<int> result)
        {
            try
            {
                meetingModel.PeopleCount = await result;

                context.Call(new StartMeetingDateTimeDialog(), BookingDateTimeResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogRes.ManyAttempts);

                SelectLocation(context);
            }
        }

        private async Task BookingDateTimeResumeAfterAsync(IDialogContext context, IAwaitable<DateTime> result)
        {
            try
            {
                meetingModel.StartDateTime = await result;

                context.Call(new DurationMeetingDialog(), DurationMeetingResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogRes.ManyAttempts);

                SelectLocation(context);
            }
        }

        private async Task DurationMeetingResumeAfterAsync(IDialogContext context, IAwaitable<TimeSpan> result)
        {
            try
            {
                meetingModel.Duration = await result;

                meetingModel.EndDateTime = meetingModel.StartDateTime + meetingModel.Duration;

                var confirmMessage = string.Format(DialogRes.MeetingSelected, meetingModel.PeopleCount,
                    meetingModel.LocationName, meetingModel.StartDateTime, meetingModel.EndDateTime);

                var options = new PromptOptions<string>(confirmMessage, tooManyAttempts: DialogRes.ManyAttempts,
                    options: new List<string>{ DialogRes.Yes, DialogRes.No }, attempts: 2);

                PromptDialog.Choice(context, ConfirmMeetingResumeAfterAsync, options);

            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogRes.ManyAttempts);

                SelectLocation(context);
            }
        }

        private static async Task ConfirmMeetingResumeAfterAsync(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = await result;

                if (message == DialogRes.Yes)
                {

                }
                else
                {
                    
                }
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogRes.ManyAttempts);
            }
            finally
            {
                await context.PostAsync("The end!");
            }
        }
    }
}