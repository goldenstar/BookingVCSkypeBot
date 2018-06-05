using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingVCSkypeBot.Authentication;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using BookingVCSkypeBot.Core.Interfaces;
using BookingVCSkypeBot.Dialogs.MeetingDateTimeDialogs;
using BookingVCSkypeBot.Models;
using BookingVCSkypeBot.Authentication.Dialogs;
using BookingVCSkypeBot.Authentication.Models;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly ITownService townService;
        private readonly IAuthProvider authProvider;
        private MeetingModel meetingModel;

        public RootDialog(ITownService townService, IAuthProvider authProvider)
        {
            this.townService = townService;
            this.authProvider = authProvider;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            // Initialize AuthenticationOptions and forward to AuthDialog for token
            var options = new AuthenticationOptions
            {
                Authority = ConfigurationManager.AppSettings["aad:Authority"],
                ClientId = ConfigurationManager.AppSettings["aad:ClientIdTest"],
                ClientSecret = ConfigurationManager.AppSettings["aad:ClientSecretTest"],
                Scopes = new[] { "User.Read" },
                RedirectUrl = ConfigurationManager.AppSettings["aad:Callback"],
                MagicNumberView = "/magic.html#{0}"
            };

            var authDialog = new AuthDialog(authProvider, options);

            await context.Forward(authDialog, AuthenticationResumeAfterAsync, message, CancellationToken.None);
        }

        private async Task AuthenticationResumeAfterAsync(IDialogContext context, IAwaitable<AuthResult> result)
        {
            var message = await result;

            SelectTown(context);
        }

        private void SelectTown(IDialogContext context)
        {
            var towns = townService.ListAll().Select(x => x.Name).ToList().AsReadOnly();

            var options = new PromptOptions<string>(DialogResource.Town, tooManyAttempts: DialogResource.ManyAttempts,
                options: towns, attempts: 2);

            PromptDialog.Choice(context, SelectTownResumeAfterAsync, options);
        }

        private async Task SelectTownResumeAfterAsync(IDialogContext context, IAwaitable<string> result)
        {
            meetingModel = new MeetingModel();

            try
            {
                var town = await result;

                meetingModel.TownName = town;

                context.Call(new PeopleCountDialog(), PeopleCountResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                SelectTown(context);
            }
        }

        private async Task PeopleCountResumeAfterAsync(IDialogContext context, IAwaitable<int> result)
        {
            try
            {
                var peopleCount = await result;

                meetingModel.PeopleCount = peopleCount;

                context.Call(new StartMeetingDateTimeDialog(), BookingDateTimeResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                SelectTown(context);
            }
        }

        private async Task BookingDateTimeResumeAfterAsync(IDialogContext context, IAwaitable<DateTime> result)
        {
            try
            {
                var dateTime = await result;

                meetingModel.StartDateTime = dateTime;

                context.Call(new DurationMeetingDialog(), DurationMeetingResumeAfterAsync);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                SelectTown(context);
            }
        }

        private async Task DurationMeetingResumeAfterAsync(IDialogContext context, IAwaitable<TimeSpan> result)
        {
            try
            {
                var duration = await result;

                meetingModel.Duration = duration;
                meetingModel.EndDateTime = meetingModel.StartDateTime + meetingModel.Duration;

                await context.PostAsync(ConfirmMessage());
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                SelectTown(context);
            }

            await context.PostAsync("The end!");
        }

        private string ConfirmMessage()
        {
            return string.Format(DialogResource.MeetingSelected, meetingModel.PeopleCount, meetingModel.TownName,
                meetingModel.StartDateTime, meetingModel.EndDateTime);
        }
    }
}