﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Connector.Services;
using System;
using Xunit;

namespace Steeltoe.Connector.MongoDb.Test
{
    public class MongoDbConnectorFactoryTest
    {
        [Fact]
        public void Constructor_ThrowsIfConfigNull()
        {
            MongoDbConnectorOptions config = null;
            MongoDbServiceInfo si = null;

            var ex = Assert.Throws<ArgumentNullException>(() => new MongoDbConnectorFactory(si, config, MongoDbTypeLocator.MongoClient));
            Assert.Contains(nameof(config), ex.Message);
        }

        [Fact]
        public void Create_ReturnsMongoDbConnection()
        {
            var config = new MongoDbConnectorOptions()
            {
                Server = "localhost",
                Port = 27016,
                Password = "password",
                Username = "username",
            };
            var si = new MongoDbServiceInfo("MyId", "mongodb://localhost:27017");
            var factory = new MongoDbConnectorFactory(si, config, MongoDbTypeLocator.MongoClient);
            var connection = factory.Create(null);
            Assert.NotNull(connection);
        }

        [Fact]
        public void UpdateConfiguration_WithNullMongoDbServiceInfo_ReturnsExpected()
        {
            var configurer = new MongoDbProviderConfigurer();
            var config = new MongoDbConnectorOptions()
            {
                Server = "localhost",
                Port = 1234,
                Username = "username",
                Password = "password",
                Database = "database"
            };
            configurer.UpdateConfiguration(null, config);

            Assert.Equal("localhost", config.Server);
            Assert.Equal(1234, config.Port);
            Assert.Equal("username", config.Username);
            Assert.Equal("password", config.Password);
            Assert.Equal("database", config.Database);
            Assert.Null(config.ConnectionString);
        }

        [Fact]
        public void UpdateConfiguration_WithMongoDbServiceInfo_ReturnsExpected()
        {
            var configurer = new MongoDbProviderConfigurer();
            var config = new MongoDbConnectorOptions()
            {
                Server = "localhost",
                Port = 1234,
                Username = "username",
                Password = "password",
                Database = "database"
            };
            var si = new MongoDbServiceInfo("MyId", "mongodb://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:27017/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355");

            configurer.UpdateConfiguration(si, config);

            Assert.Equal("192.168.0.90", config.Server);
            Assert.Equal(27017, config.Port);
            Assert.Equal("Dd6O1BPXUHdrmzbP", config.Username);
            Assert.Equal("7E1LxXnlH2hhlPVt", config.Password);
            Assert.Equal("cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355", config.Database);
        }

        [Fact]
        public void Configure_NoServiceInfo_ReturnsExpected()
        {
            var config = new MongoDbConnectorOptions()
            {
                Server = "localhost",
                Port = 1234,
                Username = "username",
                Password = "password",
                Database = "database"
            };

            var configurer = new MongoDbProviderConfigurer();
            var opts = configurer.Configure(null, config);
            Assert.Equal("mongodb://username:password@localhost:1234/database", opts);
        }

        [Fact]
        public void Configure_ServiceInfoOveridesConfig_ReturnsExpected()
        {
            var config = new MongoDbConnectorOptions()
            {
                Server = "localhost",
                Port = 1234,
                Username = "username",
                Password = "password",
                Database = "database"
            };

            var configurer = new MongoDbProviderConfigurer();
            var si = new MongoDbServiceInfo("MyId", "mongodb://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:27017/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355");

            var opts = configurer.Configure(si, config);

            Assert.Equal("mongodb://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:27017/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355", opts);
            Assert.DoesNotContain("localhost", opts);
            Assert.DoesNotContain("1234", opts);
            Assert.DoesNotContain("username", opts);
            Assert.DoesNotContain("password", opts);
            Assert.DoesNotContain("database", opts);
        }
    }
}
