using Autofac;
using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Cache;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.Optimizer;
using Microsoft.FeatureFlighting.Core.RulesEngine;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Events.CacheHandlers;
using Microsoft.FeatureFlighting.Core.Events.WebhookHandlers;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers;

namespace Microsoft.FeatureFlighting.Core
{
    /// <summary>
    /// <see cref="Module"/> for registering core dependencies
    /// </summary>
    public class CoreModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(ctx =>
            {
                var componentContext = ctx.Resolve<IComponentContext>();
                return new RequestHandlerResolver(requiredType => componentContext.Resolve(requiredType));
            }).As<IRequestHandlerResolver>();

            RegisterOperators(builder);
            RegisterServices(builder);
            RegisterQueries(builder);
            RegisterCommands(builder);
            RegisterEvents(builder);
        }

        private void RegisterOperators(ContainerBuilder builder)
        {
            builder.RegisterType<EqualOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<NotEqualOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<InOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<NotInOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<LesserThanOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<GreaterThanOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<MemberOfSecurityGroupOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<NotMemberOfSecurityGroupOperator>()
                .As<BaseOperator>()
                .SingleInstance();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureFlightCache>()
                .As<IFeatureFlightCache>()
                .SingleInstance();

            builder.RegisterType<RulesEngineManager>()
                .As<IRulesEngineManager>()
                .SingleInstance();

            builder.RegisterType<RulesEngineEvaluator>()
                .As<IRulesEngineEvaluator>()
                .SingleInstance();

            builder.RegisterType<OperatorStrategy>()
                .As<IOperatorStrategy>()
                .SingleInstance();

            builder.RegisterType<FeatureFlagEvaluator>()
                .As<IFeatureFlagEvaluator>()
                .SingleInstance();

            RegisterOptimizer(builder);
        }

        private void RegisterOptimizer(ContainerBuilder builder)
        {
            builder.RegisterType<RemoveInactiveStageOptmizationRule>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<EqualOperatorGroupingOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<InOperatorGroupingOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<NotEqualOperatorGroupingOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<NotInOperatorGroupingOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<IFlightOptimizer>()
                .As<FlightOptimizer>()
                .SingleInstance();
        }

        private void RegisterQueries(ContainerBuilder builder)
        {
            builder.RegisterType<GetAzureFeatureFlagQueryHandler>()
                .As<QueryHandler<GetAzureFeatureFlagQuery, AzureFeatureFlag?>>()
                .SingleInstance();

            builder.RegisterType<GetFeatureFlightQueryHandler>()
                .As<QueryHandler<GetFeatureFlightQuery, FeatureFlightDto>>()
                .SingleInstance();

            builder.RegisterType<GetFeatureFlightsQueryHandler>()
                .As<QueryHandler<GetFeatureFlightsQuery, IEnumerable<FeatureFlightDto>>>()
                .SingleInstance();

            builder.RegisterType<GetFeatureNamesQueryHandler>()
                .As<QueryHandler<GetFeatureNamesQuery, IEnumerable<string>>>()
                .SingleInstance();
        }

        private void RegisterCommands(ContainerBuilder builder)
        {
            builder.RegisterType<CreateFeatureFlightCommandHandler>()
                .As<CommandHandler<CreateFeatureFlightCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<UpdateFeatureFlightCommandHandler>()
                .As<CommandHandler<UpdateFeatureFlightCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<EnableFeatureFlightCommandHandler>()
                .As<CommandHandler<EnableFeatureFlightCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<DisableFeatureFlightCommandHandler>()
                .As<CommandHandler<DisableFeatureFlightCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<ActivateStageCommandHandler>()
                .As<CommandHandler<ActivateStageCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<DeleteFeatureFlightCommandHandler>()
                .As<CommandHandler<DeleteFeatureFlightCommand, IdCommandResult>>()
                .SingleInstance();
        }

        private void RegisterEvents(ContainerBuilder builder)
        {
            RegisterCacheEventHandlers(builder);
            RegisterTelemetryEventHandlers(builder);
            RegisterWebhookEventHandlers(builder);
        }

        private void RegisterCacheEventHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureFlightCreatedCacheHandler>()
                .As<EventHandler<FeatureFlightCreated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightUpdatedCacheHandler>()
                .As<EventHandler<FeatureFlightUpdated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightEnabledCacheHandler>()
                .As<EventHandler<FeatureFlightEnabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDisabledCacheHandler>()
                .As<EventHandler<FeatureFlightDisabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDeletedCacheHandler>()
                .As<EventHandler<FeatureFlightDeleted>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightStageActivatedCacheHandler>()
                .As<EventHandler<FeatureFlightStageActivated>>()
                .SingleInstance();
        }

        private void RegisterTelemetryEventHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureFlightCreatedTelemetryHandler>()
                .As<EventHandler<FeatureFlightCreated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightUpdatedTelemetryHandler>()
                .As<EventHandler<FeatureFlightUpdated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightEnabledTelemetryHandler>()
                .As<EventHandler<FeatureFlightEnabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDisabledTelemetryHandler>()
                .As<EventHandler<FeatureFlightDisabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDeletedTelemetryHandler>()
                .As<EventHandler<FeatureFlightDeleted>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightStageActivatedTelemetryHandler>()
                .As<EventHandler<FeatureFlightStageActivated>>()
                .SingleInstance();
        }

        private void RegisterWebhookEventHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureFlightCreatedWebhookHandler>()
                .As<EventHandler<FeatureFlightCreated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightUpdatedWebhookHandler>()
                .As<EventHandler<FeatureFlightUpdated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightEnabledWebhookHandler>()
                .As<EventHandler<FeatureFlightEnabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDisabledWebhookHandler>()
                .As<EventHandler<FeatureFlightDisabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightDeletedWebhookHandler>()
                .As<EventHandler<FeatureFlightDeleted>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightStageActivatedWebhookHandler>()
                .As<EventHandler<FeatureFlightStageActivated>>()
                .SingleInstance();
        }
    }
}
