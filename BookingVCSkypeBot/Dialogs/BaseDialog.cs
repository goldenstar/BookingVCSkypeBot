using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Dialogs
{
    [Serializable]
    public abstract class BaseDialog<T> : IDialog<T>
    {
        private int attempts = 3;

        public abstract Task StartAsync(IDialogContext context);

        public abstract Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result);

        protected async Task SetDialogErrorAsync(IDialogContext context, string errrorMessage)
        {
            --attempts;

            if (attempts > 0)
            {
                await context.PostAsync(errrorMessage);

                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Fail(new TooManyAttemptsException(""));
            }
        }
    }
}