﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using Steeltoe.Common;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Hypermedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Steeltoe.Management.Endpoint
{
    public static class EndPointExtensions
    {
        public static bool IsEnabled(this IEndpointOptions options, IManagementOptions mgmtOptions)
        {
            var endpointOptions = (AbstractEndpointOptions)options;

            if (endpointOptions.Enabled.HasValue)
            {
                return endpointOptions.Enabled.Value;
            }

            if (mgmtOptions.Enabled.HasValue)
            {
                return mgmtOptions.Enabled.Value;
            }

            return endpointOptions.DefaultEnabled;
        }

        public static bool IsEnabled(this IEndpoint endpoint, IManagementOptions mgmtContext)
        {
            return mgmtContext == null ? endpoint.Enabled : endpoint.Options.IsEnabled(mgmtContext);
        }

        public static bool IsExposed(this IEndpointOptions options, IManagementOptions mgmtOptions)
        {
            if (!string.IsNullOrEmpty(options.Id)
                && mgmtOptions is ActuatorManagementOptions actOptions
                && actOptions.Exposure != null)
            {
                var exclude = actOptions.Exposure.Exclude;
                if (exclude != null && (exclude.Contains("*") || exclude.Contains(options.Id)))
                {
                    return false;
                }

                var include = actOptions.Exposure.Include;
                if (include != null && (include.Contains("*") || include.Contains(options.Id)))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public static bool IsExposed(this IEndpoint endpoint, IManagementOptions mgmtContext)
        {
            return mgmtContext == null || endpoint.Options.IsExposed(mgmtContext);
        }

        public static bool RequestVerbAndPathMatch(this IEndpoint endpoint, string httpMethod, string requestPath, IEnumerable<HttpMethod> allowedMethods, IEnumerable<IManagementOptions> mgmtOptions, bool exactMatch)
        {
            return endpoint.RequestPathMatches(requestPath, mgmtOptions, out var matchingMgmtContext, exactMatch)
                && endpoint.IsEnabled(matchingMgmtContext)
                && endpoint.IsExposed(matchingMgmtContext)
                && allowedMethods.Any(m => m.Method.Equals(httpMethod));
        }

        public static IManagementOptions OptionsForContext(this IEnumerable<IManagementOptions> mgmtOptions, string requestPath, ILogger logger = null)
        {
            var match = mgmtOptions.FirstOrDefault(option => requestPath.StartsWith(option.Path));
            if (match != null)
            {
                logger?.LogTrace("Request path matched base path owned by {optionsType}", match.GetType().Name);
                return match;
            }
            else
            {
                try
                {
                    if (Platform.IsCloudFoundry)
                    {
                        return mgmtOptions.First(option => option is CloudFoundryManagementOptions);
                    }
                    else
                    {
                        return mgmtOptions.First(option => option is ActuatorManagementOptions);
                    }
                }
                catch (InvalidOperationException)
                {
                    logger?.LogError("Could not find IManagementOptions to match this request, returning first or default ActuatorManagementOptions");
                    return mgmtOptions.FirstOrDefault() ?? new ActuatorManagementOptions();
                }
            }
        }

        private static bool RequestPathMatches(this IEndpoint endpoint, string requestPath, IEnumerable<IManagementOptions> mgmtOptions, out IManagementOptions matchingContext, bool exactMatch = true)
        {
            matchingContext = null;
            var endpointPath = endpoint.Path;

            if (mgmtOptions == null)
            {
                return exactMatch ? requestPath.Equals(endpointPath) : requestPath.StartsWith(endpointPath);
            }
            else
            {
                foreach (var context in mgmtOptions)
                {
                    var contextPath = context.Path;
                    if (!contextPath.EndsWith("/") && !string.IsNullOrEmpty(endpointPath))
                    {
                        contextPath += "/";
                    }

                    var fullPath = contextPath + endpointPath;
                    if (exactMatch ? requestPath.Equals(fullPath) : requestPath.StartsWith(fullPath))
                    {
                        matchingContext = context;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
