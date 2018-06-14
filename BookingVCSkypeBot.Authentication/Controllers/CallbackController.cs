using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Threading;
using System.Reflection;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Autofac;
using BookingVCSkypeBot.Authentication.Models;

namespace BookingVCSkypeBot.Authentication.Controllers
{
    public class CallbackController : ApiController
    {
        [HttpGet]
        [Route("Callback")]
        public async Task<HttpResponseMessage> Callback()
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception());
        }

        [HttpGet]
        [Route("Callback")]
        public async Task<HttpResponseMessage> Callback([FromUri] string code, [FromUri] string state)
        {
            try
            {
                var callbackModel = GetCallbackModel(state);

                var authProvider = GetAuthProvider(callbackModel.Query);

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, callbackModel.Message))
                {
                    var store = scope.Resolve<IBotDataStore<BotData>>();
                    var keyAddress = Address.FromActivity(callbackModel.Message);
                    var userData = await store.LoadAsync(keyAddress, BotStoreType.BotUserData, CancellationToken.None);

                    var magicNumber = new Random().Next(100000, 999999);
                    var token = await GetToken(code, userData, authProvider);

                    bool successful;
                    try
                    {
                        userData.SetProperty($"{ContextKey.AuthResult}", token);
                        userData.SetProperty($"{ContextKey.MagicNumber}", magicNumber);
                        userData.SetProperty($"{ContextKey.MagicNumberValidated}", false);
                        await store.SaveAsync(keyAddress, BotStoreType.BotUserData, userData, CancellationToken.None);
                        await store.FlushAsync(keyAddress, CancellationToken.None);
                        successful = true;
                    }
                    catch (Exception)
                    {
                        successful = false;
                    }

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = GetResponseContent(successful, magicNumber)
                    };

                    await Conversation.ResumeAsync(callbackModel.ConversationReference, callbackModel.Message);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        private static StringContent GetResponseContent(bool successful, int magicNumber)
        {
            return successful
                ? new StringContent(string.Format(AuthRes.SuccessfulMagicNumberResponse, magicNumber), Encoding.UTF8, @"text/html")
                : new StringContent(AuthRes.WrongMagicNumberResponse, Encoding.UTF8, @"text/html");
        }

        private static async Task<AuthResult> GetToken(string code, BotData userData, IAuthProvider authProvider)
        {
            var authOptions = userData.GetProperty<AuthenticationOptions>($"{ContextKey.AuthOptions}");

            return await authProvider.GetTokenByAuthCodeAsync(authOptions, code);
        }

        private static CallbackModel GetCallbackModel(string state)
        {
            var model = new CallbackModel
            {
                Query = GetQuery(state)
            };

            model.ConversationReference = UrlToken.Decode<ConversationReference>(model.Query["conversationRef"]);
            model.Message = model.ConversationReference.GetPostToBotMessage();

            return model;
        }

        private static NameValueCollection GetQuery(string state)
        {
            var decoded = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(state));

            return HttpUtility.ParseQueryString(decoded);
        }

        private static IAuthProvider GetAuthProvider(NameValueCollection queryString)
        {
            var assembly = Assembly.Load(queryString["providerassembly"]);

            var type = assembly.GetType(queryString["providertype"]);

            return (IAuthProvider) Activator.CreateInstance(type);
        }
    }
}
