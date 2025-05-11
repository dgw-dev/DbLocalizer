namespace DbLocalizer.Tests.Smartling
{
    class SmartlingFileDataServiceTests : TestBase
    {
        // Arrange, Act, Assert
        // (Method)_(UnderWhatConditions)_(GivesWhatResult)

        //private List<Culture> SampleSupportedCultures => new List<Culture>()
        //{
        //    new Culture() { CultureCode = "bg-BG", SmartlingCultureCode = "bg-BG" },
        //    new Culture() { CultureCode = "es-ES", SmartlingCultureCode = "es-ES" },
        //    new Culture() { CultureCode = "zh-Hans", SmartlingCultureCode = "zh-CN" },
        //};

        //private Mock<ICultures> GetMockSupportedCultureBL(List<Culture> cultures)
        //{
        //    var mockCultures = new Mock<ICultures>();

        //    mockCultures
        //        .SetupProperty<List<Culture>>(sc => sc.CultureCollection, new List<Culture>());

        //    return mockCultures;
        //}

        //private SmartlingApiResponse<SmartlingApiFileStatus> GetFileStatusEndpointResponse(List<SmartlingApiFileStatusItem> fileStatusItems)
        //{
        //    return new SmartlingApiResponse<SmartlingApiFileStatus>()
        //    {
        //        Response = new SmartlingApiResponseInner<SmartlingApiFileStatus>()
        //        {
        //            Data = new SmartlingApiFileStatus()
        //            {
        //                Items = fileStatusItems
        //            }
        //        }
        //    };
        //}

        //private SmartlingAuthResponse GetSmartlingAuthResponse(string code, authData authData)
        //{
        //    return new SmartlingAuthResponse()
        //    {
        //        response = new authResponse()
        //        {
        //            code = code,
        //            data = authData
        //        },
        //    };
        //}
        //private SmartlingAuthResponse GetMinimalSuccessAuthResponse()
        //{
        //    return GetSmartlingAuthResponse("SUCCESS", new authData() { accessToken = "Fake Access Token" });
        //}

        //private SmartlingAuthResponse GetMinimalFailedAuthResponse()
        //{
        //    var failedAuth = GetSmartlingAuthResponse("AUTHENTICATION_ERROR", null);
        //    failedAuth.IsSuccessStatusCode = false;
        //    failedAuth.ReasonPhrase = "Test Failed Auth Response";
        //    failedAuth.StatusCode = System.Net.HttpStatusCode.InternalServerError;
        //    return failedAuth;
        //}

        //[Test]
        //public void SmartlingApiFileStatus_GettersAndSettersAreBoring()
        //{
        //    var inner = new SmartlingApiResponseInner<SmartlingApiFileStatus>();
        //    inner.Code = "code";
        //    inner.Data = new SmartlingApiFileStatus();
        //    inner.Data.FileUri = "123";

        //    Assert.AreEqual("code", inner.Code);
        //    Assert.AreEqual("123", inner.Data.FileUri);
        //}

        //[Test]
        //public void GetTranslatedFileForLocaleAsync_WhenSmartlingAuthenticationFails_ThrowsInvalidOperationException()
        //{
        //    // Arrange
        //    var minimalFailedAuth = GetMinimalFailedAuthResponse();

        //    _fileDataService
        //        .Setup(fds => fds.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(It.IsAny<SmartlingAuthorization>()).Result)
        //        .Returns(minimalFailedAuth);

        //    var service = new SmartlingFileDataService(
        //        _fileDataService.Object,
        //        new Mock<ILogger<SmartlingFileDataService>>().Object,
        //        _appSettings);

        //    // Act / Assert
        //    var supportedSmartlingCultureCode = SampleSupportedCultures.First().SmartlingCultureCode;
        //    Assert.ThrowsAsync<InvalidOperationException>(
        //        () => service.GetTranslatedFileForLocaleAsync(supportedSmartlingCultureCode, "Test File URI", Guid.Empty),
        //        $"Unable to get authorization code from smartling, StatusCode={minimalFailedAuth.StatusCode}, ReasonPhrase={minimalFailedAuth.ReasonPhrase}");
        //}

        //[Test]
        //public void GetTranslatedFileForLocaleAsync_WhenRequestedLocaleIsNotSupported_ThrowsInvalidOperationException()
        //{
        //    // Arrange
        //    var minimalFailedAuth = GetMinimalFailedAuthResponse();

        //    _fileDataService
        //        .Setup(fds => fds.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(It.IsAny<SmartlingAuthorization>()).Result)
        //        .Returns(minimalFailedAuth);

        //    var service = new SmartlingFileDataService(
        //        _fileDataService.Object,
        //        new Mock<ILogger<SmartlingFileDataService>>().Object,
        //        _appSettings);

        //    // Act / Assert
        //    var unsupportedSmartlingCultureCode = "this culture code is not real";
        //    Assert.ThrowsAsync<InvalidOperationException>(
        //        () => service.GetTranslatedFileForLocaleAsync(unsupportedSmartlingCultureCode, "Test File URI", Guid.Empty),
        //}

        //[Test]
        //public void GetTranslatedFileForLocaleAsync_WhenAllInputsAreValid_DoesNotThrow()
        //{
        //    // Arrange
        //    var minimalSuccessAuth = GetMinimalSuccessAuthResponse();

        //    string testFileUri = "Test File URI";
        //    string testCultureCode = SampleSupportedCultures.FirstOrDefault().SmartlingCultureCode;

        //    _fileDataService
        //        .Setup(x => x.GetTranslatedFile(It.IsAny<string>(), minimalSuccessAuth.response.data.accessToken, testCultureCode, testFileUri).Result)
        //        .Returns(GetMinimalImportJsonFile().ImportFileData);

        //    _fileDataService
        //        .Setup(fds => fds.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(It.IsAny<SmartlingAuthorization>()).Result)
        //        .Returns(minimalSuccessAuth);

        //    var service = new SmartlingFileDataService(
        //        _fileDataService.Object,
        //        new Mock<ILogger<SmartlingFileDataService>>().Object,
        //        _appSettings);

        //    // Act / Assert
        //    Assert.DoesNotThrowAsync(() => service.GetTranslatedFileForLocaleAsync(testCultureCode, testFileUri, Guid.Empty));
        //}



        //[Test]
        //public void GetTranslatedFilesForAllLocalesAsync_WhenUnsupportedLocaleExists_ReturnsOnlySupportedLocales()
        //{
        //    // Arrange
        //    string testFileUri = "Test File URI";
        //    string excpectedFileStatusEndpoint = _appSettings.GetSmartlingConfiguration().EndPoints.FileStatus + "?fileUri=" + testFileUri;
        //    SmartlingAuthResponse minimalSuccessfulAuth = GetMinimalSuccessAuthResponse();

        //    _fileDataService
        //        .Setup(fds => fds.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(It.IsAny<SmartlingAuthorization>()).Result)
        //        .Returns(minimalSuccessfulAuth);

        //    var fileStatusItems = SampleSupportedCultures
        //        .Select(sc => new SmartlingApiFileStatusItem() { LocaleId = sc.SmartlingCultureCode, CompletedStringCount = 100 })
        //        .ToList();

        //    // mess up the first item
        //    fileStatusItems[0].LocaleId = "unknown locale!";

        //    _fileDataService
        //        .Setup(fds => fds.GetSmartlingObjectDetails<SmartlingApiResponse<SmartlingApiFileStatus>>(excpectedFileStatusEndpoint, minimalSuccessfulAuth.response.data.accessToken).Result)
        //        .Returns(GetFileStatusEndpointResponse(fileStatusItems));

        //    var localeIds = fileStatusItems.Select(item => item.LocaleId).ToArray();

        //    _fileDataService
        //        .Setup(fds => fds.GetTranslatedFile(It.IsAny<string>(), minimalSuccessfulAuth.response.data.accessToken, It.IsIn(localeIds), testFileUri).Result)
        //        .Returns(GetMinimalImportJsonFile().ImportFileData);

        //    var service = new SmartlingFileDataService(
        //        _fileDataService.Object,
        //        new Mock<ILogger<SmartlingFileDataService>>().Object,
        //        _appSettings);

        //    // Act
        //    var response = service.GetTranslatedFilesForAllLocalesAsync(testFileUri, Guid.Empty).Result;

        //    // Assert
        //    Assert.AreEqual(fileStatusItems.Count-1, response.JsonFiles.Count); // invalid locale was removed

        //    var supportedLocaleIds = SampleSupportedCultures.Select(x => x.SmartlingCultureCode).ToList();
        //    foreach (var jsonFile in response.JsonFiles)
        //    {
        //        Assert.Contains(jsonFile.SmartlingLocale, supportedLocaleIds); // any remining results are in the supported locale list
        //    }
        //}
    }
}
