﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace Steeltoe
{
    public static class TestHelpers
    {
        public static Stream StringToStream(string str)
        {
            var memStream = new MemoryStream();
            var textWriter = new StreamWriter(memStream);
            textWriter.Write(str);
            textWriter.Flush();
            memStream.Seek(0, SeekOrigin.Begin);

            return memStream;
        }

        public static string StreamToString(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            serviceCollection.AddLogging(builder => builder.AddConsole((opts) =>
            {
#if NETCOREAPP3_1
                opts.DisableColors = true;
#endif
            }));
            serviceCollection.AddLogging(builder => builder.AddDebug());
            return serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
        }

        public static IConfiguration GetConfigurationFromDictionary(IDictionary<string, string> collection)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(collection);
            return builder.Build();
        }

        public static string EntryAssemblyName => Assembly.GetEntryAssembly().GetName().Name;

        public static readonly string VCAP_APPLICATION = @"
            {
                ""limits"": {
                    ""fds"": 16384,
                    ""mem"": 1024,
                    ""disk"": 1024
                },
                ""application_name"": ""spring-cloud-broker"",
                ""application_uris"": [
                    ""spring-cloud-broker.apps.testcloud.com""
                ],
                ""name"": ""spring-cloud-broker"",
                ""space_name"": ""p-spring-cloud-services"",
                ""space_id"": ""65b73473-94cc-4640-b462-7ad52838b4ae"",
                ""uris"": [
                    ""spring-cloud-broker.apps.testcloud.com""
                ],
                ""users"": null,
                ""version"": ""07e112f7-2f71-4f5a-8a34-db51dbed30a3"",
                ""application_version"": ""07e112f7-2f71-4f5a-8a34-db51dbed30a3"",
                ""application_id"": ""798c2495-fe75-49b1-88da-b81197f2bf06""
            }";

        public static readonly ImmutableDictionary<string, string> _fastTestsConfiguration = new Dictionary<string, string>()
        {
            { "spring:cloud:config:enabled", "false" },
            { "eureka:client:serviceUrl", "http://127.0.0.1" },
            { "eureka:client:enabled", "false" },
            { "mysql:client:ConnectionTimeout", "1" },
            { "postgres:client:timeout", "1" },
            { "redis:client:abortOnConnectFail", "false" },
            { "redis:client:connectTimeout", "1" },
            { "sqlserver:credentials:timeout", "1" },
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<string, string> _wavefrontConfiguration = new Dictionary<string, string>()
        {
             { "management:metrics:export:wavefront:uri", "proxy://localhost:7828" }
        }.ToImmutableDictionary();

#if NET6_0_OR_GREATER
        public static WebApplicationBuilder GetTestWebApplicationBuilder(string[] args = null)
        {
            var webAppBuilder = WebApplication.CreateBuilder(args);
            webAppBuilder.Configuration.AddInMemoryCollection(_fastTestsConfiguration);
            webAppBuilder.WebHost.UseTestServer();
            return webAppBuilder;
        }
#endif
    }
}
