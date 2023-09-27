using Autofac;
using Autofac.Core;
using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Cache;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.Optimizer;
using Microsoft.FeatureFlighting.Core.Evaluation;
using Microsoft.FeatureFlighting.Core.RulesEngine;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Events.CacheHandlers;
using Microsoft.FeatureFlighting.Core.Events.WebhookHandlers;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Core.Services.Cache;

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
            RegisterEvaluators(builder);
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

            builder.RegisterType<ArrayAllOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<ArrayAnyOperator>()
                .As<BaseOperator>()
                .SingleInstance();

            builder.RegisterType<NotArrayAllOperator>()
               .As<BaseOperator>()
               .SingleInstance();

            builder.RegisterType<NotArrayAnyOperator>()
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

        private void RegisterEvaluators(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureBatchBuilder>()
                .As<IFeatureBatchBuilder>()
                .SingleInstance();

            builder.RegisterType<SingleFlagEvaluator>()
                .As<ISingleFlagEvaluator>()
                .SingleInstance();

            builder.RegisterType<SyncEvaluationStrategy>()
                .As<IEvaluationStrategy>()
                .As<ISyncEvaluationStrategy>()
                .SingleInstance();

            builder.RegisterType<AsyncEvaluationStrategy>()
                .As<IEvaluationStrategy>()
                .As<IAsyncEvaluationStrategy>()
                .SingleInstance();

            builder.RegisterType<AsyncBatchEvaluationStrategy>()
                .As<IEvaluationStrategy>()
                .As<IBatchEvaluationStrategy>()
                .SingleInstance();

            builder.RegisterType<SyncBatchParallelEvaluationStrategy>()
                .As<IEvaluationStrategy>()
                .As<IBatchEvaluationStrategy>()
                .SingleInstance();

            builder.RegisterType<EvaluationStrategyBuilder>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.GetType() == typeof(IEnumerable<IEvaluationStrategy>),
                    (pi, ctx) => ctx.Resolve<IEnumerable<IEvaluationStrategy>>()))
                .As<IEvaluationStrategyBuilder>()
                .SingleInstance();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureFlightCache>()
                .As<IFeatureFlightCache>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightResultCache>()
                .As<IFeatureFlightResultCache>()
                .SingleInstance();

            builder.RegisterType<RulesEngineManager>()
                .As<IRulesEngineManager>()
                .As<IBackgroundCacheable>()
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
            builder.RegisterType<RemoveDisabledFlagStagesOptimizationRule>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<RemoveInactiveStageOptmizationRule>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<MergedEqualOperatorOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<MergedInOperatorOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<MergedNotEqualOperatorOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<MergedNotInOperatorOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<MemberOfSecurityGroupOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<NotMemberOfSecurityGroupOptimizer>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<DuplicateFilterValuesOptimizationRule>()
                .As<IFlightOptimizationRule>()
                .SingleInstance();

            builder.RegisterType<FlightOptimizer>()
                .As<IFlightOptimizer>()
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

            builder.RegisterType<GetRegisteredTenantsQueryHandler>()
                .As<QueryHandler<GetRegisteredTenantsQuery, IEnumerable<TenantConfiguration>>>()
                .SingleInstance();

            builder.RegisterType<GetEvaluationMetricsQueryHandler>()
                .As<QueryHandler<GetEvaluationMetricsQuery, EvaluationMetricsDto>>()
                .SingleInstance();

            builder.RegisterType<VerifyRulesEngineQueryHandler>()
                .As<QueryHandler<VerifyRulesEngineQuery, EvaluationResult>>()
                .SingleInstance();

            builder.RegisterType<QueryService>()
                .As<IQueryService>()
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

            builder.RegisterType<RebuildFlightsCommandHandler>()
                .As<CommandHandler<RebuildFlightsCommand, RebuildCommandResult>>()
                .SingleInstance();

            builder.RegisterType<GenerateReportCommandHandler>()
                .As<CommandHandler<GenerateReportCommand, ReportCommandResult>>()
                .SingleInstance();

            builder.RegisterType<SubscribeAlertsCommandHandler>()
                .As<CommandHandler<SubscribeAlertsCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<UnsubscribeAlertsCommandHandler>()
                .As<CommandHandler<UnsubscribeAlertsCommand, IdCommandResult>>()
                .SingleInstance();

            builder.RegisterType<UpdateMetricsCommandHandler>()
                .As<CommandHandler<UpdateMetricsCommand, MetricsCommandResult>>()
                .SingleInstance();

            builder.RegisterType<CommandBus>()
                .As<ICommandBus>()
                .SingleInstance();
        }

        private void RegisterEvents(ContainerBuilder builder)
        {
            RegisterCacheEventHandlers(builder);
            RegisterTelemetryEventHandlers(builder);
            RegisterWebhookEventHandlers(builder);

            builder.RegisterType<EventBus>()
                .As<IEventBus>()
                .SingleInstance();
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

            builder.RegisterType<FeatureFlightRebuiltTelemetryHandler>()
                .As<EventHandler<FeatureFlightRebuilt>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightAlertsEnabledTelemetryHandler>()
                .As<EventHandler<FeatureFlightAlertsEnabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightAlertsDisabledTelemetryHandler>()
                .As<EventHandler<FeatureFlightAlertsDisabled>>()
                .SingleInstance();

            builder.RegisterType<ReportGeneratedTelemetryHandler>()
                .As<EventHandler<ReportGenerated>>()
                .SingleInstance();

            builder.RegisterType<FeatureMetricsUpdatedTelemetryHandler>()
                .As<EventHandler<FeatureFlightMetricsUpdated>>()
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

            builder.RegisterType<FeatureFlightRebuiltWebhookHandler>()
                .As<EventHandler<FeatureFlightRebuilt>>()
                .SingleInstance();

            builder.RegisterType<ReportGeneratedWebhookHandler>()
                .As<EventHandler<ReportGenerated>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightAlertsEnabledWebhookHandler>()
                .As<EventHandler<FeatureFlightAlertsEnabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightAlertsDisabledWebhookHandler>()
                .As<EventHandler<FeatureFlightAlertsDisabled>>()
                .SingleInstance();

            builder.RegisterType<FeatureFlightMetricsUpdatedWebhookHandler>()
                .As<EventHandler<FeatureFlightMetricsUpdated>>()
                .SingleInstance();
        }
    }
}
