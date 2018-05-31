using System;
using System.Linq;
using System.Threading.Tasks;
using BookingVCSkypeBot.Core.Interfaces;
using BookingVCSkypeBot.Dialogs.MeetingDateTimeDialogs;
using BookingVCSkypeBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly ITownService townService;
        private MeetingModel meetingModel;

        public RootDialog(ITownService townService)
        {
            this.townService = townService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await SelectTown(context);
        }

        private async Task SelectTown(IDialogContext context)
        {
            await context.PostAsync(DialogResource.WelcomeMessage);

            var towns = townService.ListAll().Select(x => x.Name).ToList().AsReadOnly();

            var options = new PromptOptions<string>(DialogResource.Town, tooManyAttempts: DialogResource.ManyAttempts,
                options: towns, attempts: 2);

            PromptDialog.Choice(context, SelectTownResumeAfter, options);
        }

        private async Task SelectTownResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            meetingModel = new MeetingModel();

            try
            {
                var town = await result;

                meetingModel.TownName = town;

                await context.PostAsync(string.Format(DialogResource.TownSelected, town));

                context.Call(new PeopleCountDialog(), PeopleCountResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await SelectTown(context);
            }
        }

        private async Task PeopleCountResumeAfter(IDialogContext context, IAwaitable<int> result)
        {
            try
            {
                var peopleCount = await result;

                meetingModel.PeopleCount = peopleCount;

                await context.PostAsync(string.Format(DialogResource.PeopleCountSelected, peopleCount));

                context.Call(new StartMeetingDateTimeDialog(), BookingDateTimeResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                await SelectTown(context);
            }
        }

        private async Task BookingDateTimeResumeAfter(IDialogContext context, IAwaitable<DateTime> result)
        {
            try
            {
                var dateTime = await result;

                meetingModel.StartDateTime = dateTime;

                context.Call(new DurationMeetingDialog(), DurationMeetingResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                await SelectTown(context);
            }
        }

        private async Task DurationMeetingResumeAfter(IDialogContext context, IAwaitable<TimeSpan> result)
        {
            try
            {
                var duration = await result;

                meetingModel.Duration = duration;
                meetingModel.EndDateTime = meetingModel.StartDateTime + meetingModel.Duration;

                await context.PostAsync(string.Format(DialogResource.MeetingDateTimeSelected, meetingModel.StartDateTime, meetingModel.EndDateTime));
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogResource.ManyAttempts);

                await SelectTown(context);
            }

            await context.PostAsync("The end!");
        }
    
    }
}