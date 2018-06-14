using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public class PeopleCountDialog : BaseDialog<int>
    {
        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(string.Format(DialogRes.PeopleCount));

            context.Wait(MessageReceivedAsync);
        }

        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (int.TryParse(message.Text, out var peopleCount) && peopleCount > 0)
            {
                context.Done(peopleCount);
            }
            else
            {
                await SetDialogErrorAsync(context, DialogRes.PeopleCountNotValid);
            }
        }
    }
}