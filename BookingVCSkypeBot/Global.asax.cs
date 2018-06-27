using System.Web.Http;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Autofac;
using System.Reflection;

namespace BookingVCSkypeBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterBotModules();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private static void RegisterBotModules()
        {
            Conversation.UpdateContainer(builder =>
            {
                builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                builder.RegisterModule<BookingVCSkypeBotModule>();
            });
        }
    }
}