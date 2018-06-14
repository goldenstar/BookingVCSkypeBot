using System;
using System.Threading.Tasks;
using BookingVCSkypeBot.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public class DurationMeetingDialog : BaseDialog<TimeSpan>
    {
        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(string.Format(DialogRes.Duration));

            context.Wait(MessageReceivedAsync);
        }

        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.TryToHourMinuteParse(out var span))
            {
                context.Done(span);
            }
            else
            {
                await SetDialogErrorAsync(context, DialogRes.ValueNotValid);
            }
        }
    }
}