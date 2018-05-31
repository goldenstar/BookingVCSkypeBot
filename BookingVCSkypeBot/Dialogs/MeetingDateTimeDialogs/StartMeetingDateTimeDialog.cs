using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs.MeetingDateTimeDialogs
{
    [Serializable]
    public class StartMeetingDateTimeDialog : BaseDialog<DateTime>
    {
        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(string.Format(DialogResource.Date));

            context.Wait(MessageReceivedAsync);
        }

        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (DateTime.TryParse(message.Text, out var bookingDate))
            {
                if (bookingDate >= DateTime.Today)
                {
                    context.Call(new StartMeetingTimeDialog(bookingDate), ResumeAfterBookingTimeDialog);
                }
                else
                {
                    await SetDialogError(context, DialogResource.DateNotValidInPast);
                }
            }
            else
            {
                await SetDialogError(context, DialogResource.DateNotValid);
            }
        }

        private async Task ResumeAfterBookingTimeDialog(IDialogContext context, IAwaitable<DateTime> result)
        {
            var message = await result;

            context.Done(message);
        }
    }
}