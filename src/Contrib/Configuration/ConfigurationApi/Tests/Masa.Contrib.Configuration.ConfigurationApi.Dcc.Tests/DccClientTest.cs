// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Configuration.ConfigurationApi.Dcc.Tests;

[TestClass]
public class DccClientTest
{
    private const string DEFAULT_CLIENT_NAME = "masa.contrib.configuration.configurationapi.dcc";
    private Mock<IMultilevelCacheClient> _client;
    private IServiceCollection _services;
    private IServiceProvider _serviceProvider => _services.BuildServiceProvider();
    private JsonSerializerOptions _jsonSerializerOptions;
    private DccOptions _dccOptions;
    private DccSectionOptions _dccSectionOptions;
    private CustomTrigger _trigger;
    private IYamlSerializer _serializer;
    private IYamlDeserializer _deserializer;

    [TestInitialize]
    public void Initialize()
    {
        Mock<IMultilevelCacheClientFactory> multilevelCacheClientFactory = new();
        _client = new Mock<IMultilevelCacheClient>();
        _services = new ServiceCollection();
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        _dccOptions = Mock.Of<DccOptions>();
        _dccSectionOptions = new DccSectionOptions()
        {
            Environment = "Test",
            Cluster = "Default",
            AppId = "DccTest",
            ConfigObjects = new List<string>()
            {
                "Test1"
            },
            Secret = ""
        };
        _trigger = new CustomTrigger(_jsonSerializerOptions);
        _serializer = new DefaultYamlSerializer(new SerializerBuilder().JsonCompatible().Build());
        _deserializer = new DefaultYamlDeserializer(new DeserializerBuilder().Build());
        multilevelCacheClientFactory
            .Setup(factory => factory.Create("masa.contrib.configuration.configurationapi.dcc"))
            .Returns(() => _client.Object);
        _services.AddSingleton(_ => multilevelCacheClientFactory.Object);

        var serializerFactory = new Mock<ISerializerFactory>();
        var deserializerFactory = new Mock<IDeserializerFactory>();
        serializerFactory.Setup(factory => factory.Create(DEFAULT_CLIENT_NAME)).Returns(() => _serializer);
        deserializerFactory.Setup(factory => factory.Create(DEFAULT_CLIENT_NAME)).Returns(() => _deserializer);

        _services.AddSingleton(_ => serializerFactory.Object);
        _services.AddSingleton(_ => deserializerFactory.Object);
    }

    [TestMethod]
    public void TestFormatCodeErrorRawReturnThrowNotSupportedException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = "",
            FormatLabelCode = "json",
        }, _jsonSerializerOptions);
        Assert.ThrowsException<NotSupportedException>(() => client.TestFormatRaw(raw, "DccObjectName"), "configObject invalid");
    }

    [TestMethod]
    public void TestJsonFormatCodeRawReturnConfigurationTypeIsJson()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = "",
            FormatLabelCode = "Json",
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");
        Assert.IsTrue(result.ConfigurationType == ConfigurationTypes.Json);
    }

    [TestMethod]
    public void TestFormatNullRawReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(null, "DccObjectName"), "configObject invalid");
    }

    [TestMethod]
    public void TestFormatEmptyRawReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(string.Empty, "DccObjectName"), "configObject invalid");
    }

    [TestMethod]
    public void TestFormatNotSupportRawReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string raw = JsonSerializer.Serialize(new PublishRelease(), _jsonSerializerOptions);
        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(raw, "DccObjectName"), "configObject invalid");
    }

    [TestMethod]
    public void TestFormatRawByJsonReturnConfigurationTypeIsJson()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var content = JsonSerializer.Serialize(new { Name = "Microsoft" }, _jsonSerializerOptions);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = content,
            ConfigFormat = ConfigFormats.Json
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");
        Assert.IsTrue(result.Raw == content && result.ConfigurationType == ConfigurationTypes.Json);
    }

    [TestMethod]
    public void TestFormatEncryptionRawByJsonReturnConfigurationTypeIsJson()
    {
        _dccOptions.ConfigObjectSecret = "secret";
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var content = JsonSerializer.Serialize(new { Name = "Microsoft" }, _jsonSerializerOptions);
        var encryptContent = AesUtils.Encrypt(content, _dccOptions.ConfigObjectSecret, FillType.Left);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = encryptContent,
            ConfigFormat = ConfigFormats.Json,
            Encryption = true
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");
        Assert.IsTrue(result.Raw == content && result.ConfigurationType == ConfigurationTypes.Json);
    }

    [TestMethod]
    public void TestFormatEncryptionRawByJsonReturnConfigurationTypeIsJson2()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var content = JsonSerializer.Serialize(new { Name = "Microsoft" }, _jsonSerializerOptions);
        var encryptContent = AesUtils.Encrypt(content, "secret", FillType.Left);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = encryptContent,
            ConfigFormat = ConfigFormats.Json,
            Encryption = true
        }, _jsonSerializerOptions);
        Assert.ThrowsException<ArgumentNullException>(() => client.TestFormatRaw(raw, "DccObjectName"));
    }

    [TestMethod]
    public void TestFormatRawByTextReturnConfigurationTypeIsText()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var content = "Microsoft";
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = content,
            ConfigFormat = ConfigFormats.Text
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");
        Assert.IsTrue(result.Raw == content && result.ConfigurationType == ConfigurationTypes.Text);
    }

    [TestMethod]
    public void TestFormatRawByPropertiesReturnConfigurationTypeIsProperties()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var list = new List<Property>()
        {
            new()
            {
                Key = "Id",
                Value = Guid.NewGuid().ToString(),
            },
            new()
            {
                Key = "Name",
                Value = "Microsoft"
            }
        };
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = JsonSerializer.Serialize(list),
            ConfigFormat = ConfigFormats.Properties
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");
        Dictionary<string, string> dictionary = new(list.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
        Assert.IsTrue(result.Raw == JsonSerializer.Serialize(dictionary, _jsonSerializerOptions) &&
            result.ConfigurationType == ConfigurationTypes.Properties);
    }

    [TestMethod]
    public void TestFormatRawByPropertiesAndContentIsErrorReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var content = JsonSerializer.Serialize(new
        {
            Key = "Name",
            Value = "Microsoft"
        }, _jsonSerializerOptions);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = content,
            ConfigFormat = ConfigFormats.Properties
        }, _jsonSerializerOptions);
        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(raw, "DccObjectName"));
    }

    [TestMethod]
    public void TestFormatRawByXmlAndContentIsErrorReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string xml = @"<?xxx version='1.0' xx='no'?>
                    <root>
                      <name1>blazor</name>
                      <url>https://blazor.masastack.com/</url>
                      </person>
                    </root>";
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = xml,
            ConfigFormat = ConfigFormats.Xml
        }, _jsonSerializerOptions);

        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(raw, "DccObjectName"));
    }

    [TestMethod]
    public void TestFormatRawByXmlReturnConfigurationTypeIsXml()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string xml = @"<?xml version='1.0' standalone='no'?>
                    <root>
                      <person id='1'>
                          <name>blazor</name>
                          <url>https://blazor.masastack.com/</url>
                      </person>
                    </root>";
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = xml,
            ConfigFormat = ConfigFormats.Xml
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");

        Assert.IsTrue(result.ConfigurationType == ConfigurationTypes.Xml);
    }

    [TestMethod]
    public void TestFormatRawByYamlAndContentIsErrorReturnThrowArgumentException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string yaml = @"
name： Masa,
age: 1.5
addresses:
home:
    city: hangzhou";
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = yaml,
            ConfigFormat = ConfigFormats.Yaml
        }, _jsonSerializerOptions);
        Assert.ThrowsException<ArgumentException>(() => client.TestFormatRaw(raw, "DccObjectName"));
    }

    [TestMethod]
    public void TestFormatRawByYamlReturnConfigurationTypeIsXml()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string yaml = @"
name: Masa
age: 1.5
addresses:
  home:
    city: hangzhou";
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<object>(yaml);

        var serializer = new SerializerBuilder().JsonCompatible().Build();
        var json = serializer.Serialize(yamlObject);
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = yaml,
            ConfigFormat = ConfigFormats.Yaml
        }, _jsonSerializerOptions);
        var result = client.TestFormatRaw(raw, "DccObjectName");

        Assert.IsTrue(result.Raw == json && result.ConfigurationType == ConfigurationTypes.Yaml);
    }

    [TestMethod]
    public async Task TestGetRawByKeyAsyncReturnConfigurationTypeIsText()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        string content = "Microsoft";
        string raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = content,
            ConfigFormat = ConfigFormats.Text
        }, _jsonSerializerOptions);
        string key = "DccObjectName";
        bool isExecute = false;
        _client
            .Setup(c => c.GetAsync(key, It.IsAny<Action<string>>()!, null))
            .ReturnsAsync(raw)
            .Callback((string value, Action<string> action, Action<CacheOptions>? cacheOptionsAction) =>
            {
                _trigger.Formats = ConfigFormats.Text;
                _trigger.Content = JsonSerializer.Serialize(new PublishRelease()
                {
                    Content = "Apple",
                    ConfigFormat = ConfigFormats.Text
                }, _jsonSerializerOptions);
                _trigger.Action = action;
            });
        var result = await client.TestGetRawByKeyAsync(key, message =>
        {
            isExecute = true;
        });
        Assert.IsTrue(result.ConfigurationType == ConfigurationTypes.Text && result.Raw == content);
        Assert.IsFalse(isExecute);
        _trigger.Execute();
        Assert.IsTrue(isExecute);
    }

    [TestMethod]
    public void TestGetDynamicAsyncByEmptyKeyReturnThrowArgumentNullException()
    {
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        Assert.ThrowsExceptionAsync<ArgumentNullException>(() => client.TestGetDynamicAsync(string.Empty, null));
    }

    [TestMethod]
    public async Task TestGetDynamicAsyncReturnResultNameIsApple()
    {
        string key = "environment-cluster-appId-configObject";
        var raw = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = JsonSerializer.Serialize(new
            {
                id = "1",
                name = "Apple"
            }, _jsonSerializerOptions),
            ConfigFormat = ConfigFormats.Json
        }, _jsonSerializerOptions);
        _client
            .Setup(c => c.GetAsync<string>(key, It.IsAny<Action<string>>()!, null))
            .ReturnsAsync(raw)
            .Callback((string str, Action<string> action, Action<CacheOptions>? cacheOptionsAction) =>
            {
                _trigger.Formats = ConfigFormats.Json;
                _trigger.Content = JsonSerializer.Serialize(new PublishRelease()
                {
                    Content = JsonSerializer.Serialize(new
                    {
                        id = "1",
                        name = "HuaWei"
                    }, _jsonSerializerOptions),
                    ConfigFormat = ConfigFormats.Json
                }, _jsonSerializerOptions);
                _trigger.Action = action;
            });
        bool isExecute = false;
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        dynamic result = await client.TestGetDynamicAsync(key, (key, value, options) =>
        {
            isExecute = true;
        });
        Assert.IsTrue(result.name == "Apple");
        Assert.IsTrue(result.id == "1");
        _trigger.Execute();
        Assert.IsTrue(isExecute);
    }

    [DataTestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task TaskGetDynamicAsyncReturnResultCountIs2(string environment, string cluster, string appId, string configObject)
    {
        var brand = new List<Property>()
        {
            new()
            {
                Key = "Id",
                Value = Guid.NewGuid().ToString(),
            },
            new()
            {
                Key = "Name",
                Value = "Microsoft"
            }
        };
        _client.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null))
            .ReturnsAsync(() => new PublishRelease()
            {
                ConfigFormat = ConfigFormats.Properties,
                Content = brand.Serialize(_jsonSerializerOptions)
            }.Serialize(_jsonSerializerOptions))
            .Callback((string value, Action<string> action, Action<CacheOptions>? cacheOptionsAction) =>
            {
                _trigger.Formats = ConfigFormats.Properties;
                _trigger.Content = new List<Property>()
                {
                    new()
                    {
                        Key = "Id",
                        Value = Guid.NewGuid().ToString(),
                    },
                    new()
                    {
                        Key = "Name",
                        Value = "HuaWei"
                    }
                }.Serialize(_jsonSerializerOptions);
                _trigger.Action = action;
            })
            .Verifiable();
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        bool isExecute = false;
        var result = await client.GetDynamicAsync(environment, cluster, appId, configObject, value => isExecute = true);
        _trigger.Execute();
        Assert.IsTrue((result as ExpandoObject)!.Count() == 2);
        Assert.IsTrue(isExecute);
    }

    [TestMethod]
    public async Task TaskGetDynamicAsyncByKeyReturnResultCountIs1()
    {
        var brand = new List<Property>()
        {
            new()
            {
                Key = "Id",
                Value = Guid.NewGuid().ToString(),
            }
        };
        _client.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null))
            .ReturnsAsync(() => new PublishRelease
            {
                ConfigFormat = ConfigFormats.Properties,
                Content = brand.Serialize(_jsonSerializerOptions)
            }.Serialize(_jsonSerializerOptions))
            .Verifiable();
        var client = new CustomConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);

        string key = "environment-cluster-appId-configObject";
        var result = await client.GetDynamicAsync(key);
        _trigger.Execute();
        Assert.IsTrue((result as ExpandoObject)!.Count() == 1);
    }

    [TestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task TestGetAsyncByJsonReturn(string environment, string cluster, string appId, string configObject)
    {
        var brand = new Brands("Microsoft");
        var newBrand = new Brands("Microsoft2");

        _client
            .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null))
            .ReturnsAsync(()
                => new PublishRelease()
                {
                    ConfigFormat = ConfigFormats.Json,
                    Content = brand.Serialize(_jsonSerializerOptions)
                }.Serialize(_jsonSerializerOptions))
            .Callback((string str, Action<string> action, Action<CacheOptions>? cacheOptionsAction) =>
            {
                _trigger.Formats = ConfigFormats.Json;
                _trigger.Content = newBrand.Serialize(_jsonSerializerOptions);
                _trigger.Action = action;
            });

        var client = new ConfigurationApiClient(_serviceProvider, _jsonSerializerOptions, _dccOptions, _dccSectionOptions, null);
        var ret = await client.GetAsync(environment, cluster, appId, configObject, (Brands br) =>
        {
            Assert.IsTrue(br.Id == newBrand.Id);
            Assert.IsTrue(br.Name == newBrand.Name);
        });
        Assert.IsNotNull(ret);
        Assert.IsTrue(brand.Id == ret.Id && brand.Name == ret.Name);
        _trigger.Execute();

        ret = await client.GetAsync(environment, cluster, appId, configObject, It.IsAny<Action<Brands>>());
        Assert.IsNotNull(ret);

        Assert.IsTrue(ret.Id == newBrand.Id && ret.Name == newBrand.Name);
    }

    [DataTestMethod]
    [DataRow("Development", "Default", "WebApplication1", "Brand")]
    public void TestSingleSection(string environment, string cluster, string appId, string configObject)
    {
        CustomTrigger trigger = new CustomTrigger(_jsonSerializerOptions);
        var brand = new Brands("Microsoft");
        var response = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = brand.Serialize(_jsonSerializerOptions),
            ConfigFormat = ConfigFormats.Text
        });
        Mock<IMultilevelCacheClient> memoryCacheClient = new();
        memoryCacheClient
            .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null).Result)
            .Returns(() => response);

        Mock<IMultilevelCacheClientFactory> memoryCacheClientFactory = new();
        memoryCacheClientFactory
            .Setup(factory => factory.Create(DEFAULT_CLIENT_NAME))
            .Returns(() => memoryCacheClient.Object);
        _services.AddSingleton(_ => memoryCacheClientFactory.Object);

        var configurationApiClient = new ConfigurationApiClient(
            _services.BuildServiceProvider(),
            _jsonSerializerOptions,
            _dccOptions,
            new Mock<DccSectionOptions>().Object,
            new List<DccSectionOptions>());
        _services.AddSingleton<IConfigurationApiClient>(configurationApiClient);

        Assert.IsTrue(
            configurationApiClient
                .GetRawAsync(environment, cluster, appId, configObject, It.IsAny<Action<string>>())
                .GetAwaiter()
                .GetResult().Raw == brand.Serialize(_jsonSerializerOptions));
        trigger.Execute();
    }

    [DataTestMethod]
    [DataRow("Development", "Default", "WebApplication1", "Brand")]
    public void TestSingleSection2(string environment, string cluster, string appId, string configObject)
    {
        Mock<IMultilevelCacheClient> memoryCacheClient = new();
        Dictionary<string, string> masaDic = new Dictionary<string, string>()
        {
            { "Id", Guid.NewGuid().ToString() },
            { "Name", "Masa" }
        };
        var response = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = masaDic.Serialize(_jsonSerializerOptions),
            ConfigFormat = ConfigFormats.Json
        });
        memoryCacheClient
            .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null).Result)
            .Returns(() => response);

        Mock<IMultilevelCacheClientFactory> memoryCacheClientFactory = new();
        memoryCacheClientFactory
            .Setup(factory => factory.Create(DEFAULT_CLIENT_NAME))
            .Returns(() => memoryCacheClient.Object);
        _services.AddSingleton(_ => memoryCacheClientFactory.Object);

        var configurationApiClient = new ConfigurationApiClient(
            _services.BuildServiceProvider(),
            _jsonSerializerOptions,
            _dccOptions,
            new Mock<DccSectionOptions>().Object,
            new List<DccSectionOptions>());
        _services.AddSingleton<IConfigurationApiClient>(configurationApiClient);

        Assert.IsTrue(
            configurationApiClient.GetRawAsync(
                environment,
                cluster,
                appId,
                configObject,
                It.IsAny<Action<string>>()).Result.Raw == masaDic.Serialize(_jsonSerializerOptions));
    }

    [DataTestMethod]
    [DataRow("Development", "Default", "WebApplication1", "Brand")]
    public void TestSingleSection3(string environment, string cluster, string appId, string configObject)
    {
        Mock<IMultilevelCacheClient> memoryCacheClient = new();

        var response = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = "Test",
            ConfigFormat = ConfigFormats.Text
        });
        memoryCacheClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null).Result)
            .Returns(() => response);

        Mock<IMultilevelCacheClientFactory> memoryCacheClientFactory = new();
        memoryCacheClientFactory
            .Setup(factory => factory.Create(DEFAULT_CLIENT_NAME))
            .Returns(() => memoryCacheClient.Object);
        _services.AddSingleton(_ => memoryCacheClientFactory.Object);

        var configurationApiClient = new ConfigurationApiClient(
            _services.BuildServiceProvider(),
            _jsonSerializerOptions,
            _dccOptions,
            new Mock<DccSectionOptions>().Object,
            new List<DccSectionOptions>());
        _services.AddSingleton<IConfigurationApiClient>(configurationApiClient);

        Assert.IsTrue(configurationApiClient.GetRawAsync(
            environment,
            cluster,
            appId,
            configObject,
            It.IsAny<Action<string>>()).GetAwaiter().GetResult().Raw == "Test");
    }

    [DataTestMethod]
    [DataRow("Development", "Default", "WebApplication1", "Brand")]
    public void TestSingleSection4(string environment, string cluster, string appId, string configObject)
    {
        Mock<IMultilevelCacheClient> memoryCacheClient = new();

        var response = JsonSerializer.Serialize(new PublishRelease()
        {
            Content = null,
            ConfigFormat = ConfigFormats.Text
        });
        memoryCacheClient
            .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null).Result)
            .Returns(() => response);

        Mock<IMultilevelCacheClientFactory> memoryCacheClientFactory = new();
        memoryCacheClientFactory
            .Setup(factory => factory.Create(DEFAULT_CLIENT_NAME))
            .Returns(() => memoryCacheClient.Object);
        _services.AddSingleton(_ => memoryCacheClientFactory.Object);

        var configurationApiClient = new ConfigurationApiClient(
            _services.BuildServiceProvider(),
            _jsonSerializerOptions,
            _dccOptions,
            new Mock<DccSectionOptions>().Object,
            new List<DccSectionOptions>());
        _services.AddSingleton<IConfigurationApiClient>(configurationApiClient);

        Assert.IsTrue(configurationApiClient.GetRawAsync(
            environment,
            cluster,
            appId,
            configObject,
            It.IsAny<Action<string>>()).GetAwaiter().GetResult().Raw == null);
    }

    [DataTestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task GetAsyncByProperty(string environment, string cluster, string appId, string configObject)
    {
        var brand = new List<Property>()
        {
            new()
            {
                Key = "Id",
                Value = Guid.NewGuid().ToString(),
            },
            new()
            {
                Key = "Name",
                Value = "Microsoft"
            }
        };
        _client
            .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<Action<string?>>(), null).Result)
            .Returns(() => new PublishRelease()
            {
                ConfigFormat = ConfigFormats.Properties,
                Content = brand.Serialize(_jsonSerializerOptions)
            }.Serialize(_jsonSerializerOptions)).Verifiable();
        var client = new ConfigurationApiClient(
            _serviceProvider,
            _jsonSerializerOptions,
            _dccOptions,
            _dccSectionOptions,
            null);
        var ret = await client.GetAsync(environment, cluster, appId, configObject, It.IsAny<Action<Brands>>());
        Assert.IsNotNull(ret);

        Assert.IsTrue(ret.Id.ToString() == brand.Where(b => b.Key == "Id").Select(t => t.Value).FirstOrDefault() &&
            ret.Name == brand.Where(b => b.Key == "Name").Select(t => t.Value).FirstOrDefault());
    }
}
