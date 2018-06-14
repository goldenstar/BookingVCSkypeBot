using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = scope.Resolve<IDialog<object>>();
                    await Conversation.SendAsync(activity, () => dialog);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}