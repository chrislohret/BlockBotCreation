using System;
using Microsoft.Xrm.Sdk;

namespace BlockEnvironmentCopilot
{
    /// <summary>
    /// Base class for all plug-ins. Handles the boilerplate of extracting the
    /// execution context and services, then delegates work to <see cref="ExecuteDataversePlugin"/>.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        protected string PluginClassName { get; }

        /// <summary>
        /// Optional unsecure configuration string registered with the plugin step.
        /// </summary>
        protected string UnsecureConfiguration { get; }

        /// <summary>
        /// Optional secure configuration string registered with the plugin step.
        /// </summary>
        protected string SecureConfiguration { get; }

        protected PluginBase(string unsecureConfiguration, string secureConfiguration)
        {
            PluginClassName = GetType().ToString();
            UnsecureConfiguration = unsecureConfiguration;
            SecureConfiguration = secureConfiguration;
        }

        /// <summary>
        /// Main entry point invoked by the Dataverse platform.
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var localContext = new LocalPluginContext(serviceProvider);

            try
            {
                localContext.Trace($"Entered {PluginClassName}.Execute()");

                ExecuteDataversePlugin(localContext);
            }
            catch (InvalidPluginExecutionException)
            {
                // Business-rule exceptions bubble straight up to the platform.
                throw;
            }
            catch (Exception ex)
            {
                localContext.Trace($"Exception in {PluginClassName}: {ex}");
                throw new InvalidPluginExecutionException(
                    $"An error occurred in {PluginClassName}. See trace log for details.", ex);
            }
            finally
            {
                localContext.Trace($"Exiting {PluginClassName}.Execute()");
            }
        }

        /// <summary>
        /// Implement this method with your plugin logic.
        /// </summary>
        protected abstract void ExecuteDataversePlugin(ILocalPluginContext localContext);
    }
}
