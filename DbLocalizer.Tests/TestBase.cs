using Entities;
using Entities.DAL;
using Entities.Interfaces;
using Entities.Plugins.TranslationManagement.Smartling;
using Entities.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbLocalizer.Tests
{
    public abstract class TestBase
    {
        protected IConfigurationRoot _configuration;
        protected bool _translationServiceIsRunning = false;
        protected AppSettings _appSettings;

        // When adding new mocks here, do not forget to call .Reset() in SetUpBase()
        protected readonly Mock<IGenericRepository> _genericRepository = new();
        protected readonly Mock<IWebHostEnvironment> _mockIWebHostEnvironment = new();
        protected readonly Mock<IFileDataService> _fileDataService = new();
        protected readonly Mock<IExportDal> _exportDal = new();
        protected readonly Mock<ILogger> _logger = new();
        protected readonly Mock<IBackgroundWorkerQueue> _backgroundJobQueue = new();
        protected readonly Mock<ISmartlingFileDataService> _smartlingFileDataService = new();
        protected readonly Mock<ILongRunningService> _longRunningService = new();
        protected readonly Mock<ICacheManager> _cacheManager = new();
        protected readonly Mock<IConfiguration> _config = new();
        protected readonly Mock<ISqlSchemaBuilder> _sqlSchemaBuilder = new();
        protected readonly Mock<ISmartlingDataService> _smartlingDataService = new();

        protected string ConnectionString = string.Empty;

        private static string ContentRootPath
        {
            get
            {
                string rootPath = @"..\..\..";
                string fullPath = Path.Combine(Environment.CurrentDirectory, rootPath);
                return Path.GetFullPath(fullPath);
            }
        }

        private static string AppsettingsTestPath => Path.Combine(ContentRootPath, "appsettings.Test.json");
        private static string CulturesTestPath => Path.Combine(ContentRootPath, "cultures.Test.json");
        private static string DatabasesTestPath => Path.Combine(ContentRootPath, "databases.Test.json");
        private static string SmartlingSettingsTestPath => Path.Combine(ContentRootPath, "Smartling", "smartlingSettings.Test.json");

        protected TestBase()
        {
            SetUpBase();
        }

        [SetUp]
        public void SetUpBase()
        {
            // Reset mocks between tests
            _genericRepository.Reset();
            _mockIWebHostEnvironment.Reset();
            _fileDataService.Reset();
            _exportDal.Reset();
            _logger.Reset();
            _backgroundJobQueue.Reset();
            _smartlingFileDataService.Reset();
            _longRunningService.Reset();
            _cacheManager.Reset();
            _config.Reset();
            _sqlSchemaBuilder.Reset();
            _smartlingDataService.Reset();

            // Setup _mockIWebHostEnvironment and _configuration
            SetupMockWebHost();
            SetupConfiguration();

            // Reset configuration between tests
            _appSettings = _configuration.Get<AppSettings>();
        }

        private void SetupMockWebHost()
        {
            _mockIWebHostEnvironment.Setup(m => m.ContentRootPath).Returns(ContentRootPath);
            _mockIWebHostEnvironment.Setup(m => m.EnvironmentName).Returns("UnitTest");
        }

        private void SetupConfiguration()
        {
            // Only read appsettings.Test.json once
            if (_configuration is null)
            {
                _configuration = ReadTestConfiguration();
            }
        }

        protected IConfigurationRoot ReadTestConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(AppsettingsTestPath)
                .AddJsonFile(CulturesTestPath)
                .AddJsonFile(DatabasesTestPath)
                .AddJsonFile(SmartlingSettingsTestPath).Build();
        }

        protected CancellationTokenSource GetCancellationTokenSource(Guid key)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            if (!TokenStore.Store.TryGetValue(key, out CancellationTokenSource source))
            {
                TokenStore.Store.Add(key, tokenSource);
            }

            return tokenSource;
        }

        protected static void DestroyToken(Guid key)
        {
            if (TokenStore.Store.TryGetValue(key, out CancellationTokenSource source))
            {
                source.Cancel();
                TokenStore.Store.Remove(key);
            }
        }
        protected static async Task<DataTable> GetBaseTable()
        {
            // Create a new DataTable
            DataTable table = new DataTable("SampleBaseTable");

            // Define columns
            table.Columns.Add("TableName", typeof(string));
            table.Columns.Add("FullTableName", typeof(string));
            table.Columns.Add("FullCultureTableName", typeof(string));
            table.Columns.Add("TableSchema", typeof(string));

            // Add rows
            table.Rows.Add("Products", "[dbo].[Products]", "[dbo].[ProductsCulture]", "dbo");

            return await Task.FromResult(table);
        }
    }
}
