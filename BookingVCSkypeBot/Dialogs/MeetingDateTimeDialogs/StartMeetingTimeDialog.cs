using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs.MeetingDateTimeDialogs
{
    [Serializable]
    public class StartMeetingTimeDialog : BaseDialog<DateTime>
    {
        private readonly DateTime bookingDate;

        public StartMeetingTimeDialog(DateTime bookingDate)
        {
            this.bookingDate = bookingDate;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(string.Format(DialogRes.Time));

            context.Wait(MessageReceivedAsync);
        }

        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (DateTime.TryParseExact(message.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                var fullDateTime = GetFullDateTime(time);

                if (fullDateTime >= DateTime.Now)
                {
                    context.Done(fullDateTime);
                }
                else
                {
                    await SetDialogErrorAsync(context, DialogRes.DateNotValidInPast);
                }
            }
            else
            {
                await SetDialogErrorAsync(context, DialogRes.ValueNotValid);
            }
        }

        private DateTime GetFullDateTime(DateTime time)
        {
            return new DateTime(bookingDate.Year, bookingDate.Month, bookingDate.Day, time.Hour, time.Minute, 0);
        }
    }
}