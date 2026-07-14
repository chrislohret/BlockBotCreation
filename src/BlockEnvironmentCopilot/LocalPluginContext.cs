using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;

namespace BlockEnvironmentCopilot
{
    /// <summary>
    /// Convenience wrapper exposing the services a plugin commonly needs.
    /// </summary>
    public interface ILocalPluginContext
    {
        /// <summary>The plugin execution context.</summary>
        IPluginExecutionContext PluginExecutionContext { get; }

        /// <summary>Organization service running in the context of the calling user.</summary>
        IOrganizationService InitiatingUserService { get; }

        /// <summary>Organization service running in the context of the SYSTEM user.</summary>
        IOrganizationService SystemUserService { get; }

        /// <summary>Notification / tracing service.</summary>
        ITracingService TracingService { get; }

        /// <summary>Structured logger (ILogger) for Application Insights integration.</summary>
        ILogger Logger { get; }

        /// <summary>Writes a message to the plugin trace log.</summary>
        void Trace(string message);
    }

    /// <summary>
    /// Default implementation of <see cref="ILocalPluginContext"/>.
    /// </summary>
    public sealed class LocalPluginContext : ILocalPluginContext
    {
        public IPluginExecutionContext PluginExecutionContext { get; }

        public IOrganizationService InitiatingUserService { get; }

        public IOrganizationService SystemUserService { get; }

        public ITracingService TracingService { get; }

        public ILogger Logger { get; }

        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));

            PluginExecutionContext =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            TracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Service impersonating the calling user.
            InitiatingUserService = factory.CreateOrganizationService(PluginExecutionContext.InitiatingUserId);

            // Service running as the SYSTEM user (null userId).
            SystemUserService = factory.CreateOrganizationService(null);
        }

        public void Trace(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }

            if (PluginExecutionContext == null)
            {
                TracingService.Trace(message);
            }
            else
            {
                TracingService.Trace(
                    "{0}, Correlation Id: {1}, Initiating User: {2}",
                    message,
                    PluginExecutionContext.CorrelationId,
                    PluginExecutionContext.InitiatingUserId);
            }
        }
    }
}
