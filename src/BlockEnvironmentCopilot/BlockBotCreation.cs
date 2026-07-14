using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace BlockEnvironmentCopilot
{
    /// <summary>
    /// Blocks creation of Copilot agents (the <c>bot</c> table) in the environment
    /// where this plugin is registered.
    ///
    /// Deploy and register this step ONLY in the Default Environment so that agent
    /// creation is prevented there while remaining allowed elsewhere.
    ///
    /// Registration:
    ///   Message:        Create
    ///   Primary Entity: bot
    ///   Stage:          PreValidation (runs before the operation, outside the transaction)
    ///   Execution Mode: Synchronous
    /// </summary>
    public sealed class BlockBotCreation : PluginBase
    {
        /// <summary>
        /// Schema name of the Dataverse environment variable that, when present,
        /// overrides <see cref="DefaultBlockMessage"/>.
        /// </summary>
        private const string BlockMessageVariableSchemaName = "bbc_MakerUIMessage";

        /// <summary>
        /// Fallback message used when the environment variable is missing or empty.
        /// </summary>
        private const string DefaultBlockMessage =
            "Copilot agent creation is restricted in this environment. " +
            "Please create your agent in an approved Power Platform environment " +
            "or contact your administrator for assistance.";

        public BlockBotCreation(string unsecureConfiguration, string secureConfiguration)
            : base(unsecureConfiguration, secureConfiguration)
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            localContext.Trace("Blocking Copilot agent (bot) creation.");

            var blockMessage = GetEnvironmentVariableValue(
                localContext,
                BlockMessageVariableSchemaName) ?? DefaultBlockMessage;

            // Prevent Copilot agent creation in this environment.
            throw new InvalidPluginExecutionException(blockMessage);
        }

        /// <summary>
        /// Reads the value of a Dataverse environment variable by schema name.
        /// Returns the current value if set, otherwise the definition's default
        /// value, or <c>null</c> if the variable does not exist or has no value.
        /// </summary>
        private static string GetEnvironmentVariableValue(ILocalPluginContext localContext, string schemaName)
        {
            var service = localContext.SystemUserService;

            var query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue"),
                TopCount = 1,
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("schemaname", ConditionOperator.Equal, schemaName)
                    }
                }
            };

            // Left-join the current value (an override of the default), if any.
            var valueLink = query.AddLink(
                "environmentvariablevalue",
                "environmentvariabledefinitionid",
                "environmentvariabledefinitionid",
                JoinOperator.LeftOuter);
            valueLink.Columns = new ColumnSet("value");
            valueLink.EntityAlias = "v";

            var results = service.RetrieveMultiple(query);
            if (results.Entities.Count == 0)
            {
                localContext.Trace($"Environment variable '{schemaName}' was not found.");
                return null;
            }

            var definition = results.Entities[0];

            // Prefer the current value over the default value.
            if (definition.Contains("v.value")
                && definition["v.value"] is AliasedValue aliased
                && aliased.Value is string currentValue
                && !string.IsNullOrWhiteSpace(currentValue))
            {
                localContext.Trace($"Using current value of environment variable '{schemaName}'.");
                return currentValue;
            }

            var defaultValue = definition.GetAttributeValue<string>("defaultvalue");
            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                localContext.Trace($"Using default value of environment variable '{schemaName}'.");
                return defaultValue;
            }

            localContext.Trace($"Environment variable '{schemaName}' has no value; using built-in default.");
            return null;
        }
    }
}
