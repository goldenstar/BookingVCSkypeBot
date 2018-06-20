using System;
using System.Configuration;
using Autofac;
using BookingVCSkypeBot.Authentication;
using BookingVCSkypeBot.Authentication.AADv2;
using BookingVCSkypeBot.Core.Interfaces;
using BookingVCSkypeBot.Core.Services;
using BookingVCSkypeBot.Dialogs;
using BookingVCSkypeBot.Infrastructure.Data;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot
{
    public class BookingVCSkypeBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterAuth(builder);

            RegisterDialogs(builder);

            RegisterDB(builder);

            RegisterServices(builder);
        }

        private static void RegisterDialogs(ContainerBuilder builder)
        {
            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();

            builder.Register(c => new CancelScorable(c.Resolve<IDialogTask>()))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterDB(ContainerBuilder builder)
        {
            builder.RegisterType<BookingVCContext>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(EfRepository<>))
                .As(typeof(IRepository<>))
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<LocationService>()
                .Keyed<ILocationService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<VCService>()
                .Keyed<IVCService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private static void RegisterAuth(ContainerBuilder builder)
        {
            builder.RegisterType<AuthProvider>()
                .As<IAuthProvider>()
                .InstancePerDependency();

            //var store = new TableBotDataStore(ConfigurationManager.AppSettings["AzureWebJobsStorage"]); // PUBLISH

            var docDbServiceEndpoint = new Uri(ConfigurationManager.AppSettings["DocumentDbServiceEndpoint"]); // LOCAL
            var docDbEmulatorKey = ConfigurationManager.AppSettings["DocumentDbAuthKey"];
            var store = new DocumentDbBotDataStore(docDbServiceEndpoint, docDbEmulatorKey);

            builder.Register(c => store)
                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                .AsSelf()
                .SingleInstance();
        }
    }
}