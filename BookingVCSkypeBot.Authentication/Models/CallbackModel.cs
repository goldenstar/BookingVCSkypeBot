using System.Collections.Specialized;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Authentication.Models
{
    public class CallbackModel
    {
        public NameValueCollection Query { get; set; }
        public ConversationReference ConversationReference { get; set; }
        public Activity Message { get; set; }
    }
}