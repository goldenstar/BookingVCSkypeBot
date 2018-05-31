using Autofac;
using BookingVCSkypeBot.Core.Interfaces;
using BookingVCSkypeBot.Core.Services;
using BookingVCSkypeBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace BookingVCSkypeBot
{
    public class CustomModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();

            // Service dependencies
            builder.RegisterType<TownService>()
                .Keyed<ITownService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<VCService>()
                .Keyed<IVCService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}