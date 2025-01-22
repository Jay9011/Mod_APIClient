# API Client 라이브러리 사용 설명서

## 목차
1. [개요](#개요)
2. [설치](#설치)
3. [인증 방식](#인증-방식)
4. [API 요청 구성](#api-요청-구성)
5. [API 응답 처리](#api-응답-처리)
6. [재시도 정책](#재시도-정책)
7. [설정 관리](#설정-관리)
8. [사용 예시](#사용-예시)

## 개요

API Client는 HTTP API 클라이언트를 쉽게 구현할 수 있도록 도와주는 .NET 기반의 라이브러리입니다. REST API를 호출하고 반환되는 데이터를 원하는 형식으로 받을 수 있습니다.

- **API 설정 관리** : BaseUrl과 Route에 해당하는 Endpoints 설정을 관리
- **다양한 인증 방식 지원** : Bearer, Basic, ApiKey, Cookie 등...
- **요청/응답 직렬화 및 역직렬화** : JSON, Form Data, URL Encoded 등으로 요청을 자동 직렬화 하고, 응답을 Generic 모델로 자동 역직렬화
- **자동 재시도 정책** : 응답 시간 경과시 재시도 정책을 사용하도록 구현...(예: 3회 재시도, 2초 간격)
- **로깅** : 필요한 경우 요청/응답 로그를 남기도록 구현

## 설치

### 1. API 설정 구성

```csharp
var connectionInfo = new APIConnectionInfo
{
    BaseUrl = "https://api.example.com",    // API Base URL (포트를 포함한 전체 공통 URL)
    Endpoints = new Dictionary<string, string>  // API 엔드포인트 설정 (실제 API 경로)
    {
        ["GetData"] = "api/data",
        ["CreateItem"] = "api/items",
        ["UpdateItem"] = "api/items/{id}"
    }
};

var apiSetup = new APISetup(ioHelper, "API_NAME");  // API 설정 생성 (IOHelper를 통해 설정 입/출력)
apiSetup.UpdateConnectionInfo(connectionInfo);  // API 설정 저장/업데이트
```

### 2. API 클라이언트 생성

```csharp
var apiSetup = new APISetup(ioHelper, "API_NAME");
ICoreAPI api = new CoreAPIClient(apiSetup); // 설정을 통해 원하는 Endpoint로 API를 호출하는 API 클라이언트 생성
```

### 3. API 요청 생성
```csharp
IAPIRequest request = new APIRequest
{
    Authentication = new BearerTokenAuthentication(token),  // 인증 방식 설정 예시
    Body = RequestBody.CreateFormData(formModel)    // 요청 본문 설정 예시
}
```

### 4. API 호출 및 응답 처리
```csharp
IAPIResponse<T> response = await api.SendAsync<T>(request);  // API 호출시 범용 SendAsync 메서드 사용 후 응답 처리
```

## 인증 방식

### Basic 인증 (검증 필요)

```csharp
var auth = BasicAuthentication.FromUserPass("username", "password");
```

### Bearer Token 인증

```csharp
var auth = new BearerTokenAuthentication("your-token");
```

### API Key 인증 (검증 필요)

```csharp
var auth = new ApiKeyAuthentication("your-api-key", "X-API-KEY");
```

### Cookie 인증 (검증 필요)

```csharp
var auth = new CookieAuthentication("cookie-name", "cookie-value");
```

### Custom 인증 (검증 필요)

```csharp
var auth = new CustomAuthentication("header-key", "header-value");
```

## API 요청 구성

### 기본 요청 생성

```csharp
IAPIRequest requestParam = new APIRequest
{
    Method = HttpMethod.Post,                       // 요청 메서드 설정 (기본값: GET, 이후 API 클라이언트의 호출을 통해서도 바꿀 수 있으니 생략 가능)
    PathParameters = new Dictionary<string, string> // 경로 파라미터 설정 (없으면 생략 가능)
    {
        ["VisitStatusID"] = model.VisitStatusID
    },
    Authentication = new BearerTokenAuthentication(token),  // 인증 방식 설정 (없으면 생략 가능)
    Headers = new Dictionary<string, string>                // 헤더 설정 (없으면 생략 가능)
    {
        ["Content-Type"] = "application/x-www-form-urlencoded"
    },
    Body = RequestBody.CreateFormData(formModel),       // 요청 본문 설정 (없으면 생략 가능)
    RetryCount = 3,                                     // 재시도 횟수 설정 (기본값: 3, 없으면 생략 가능)
    TimeoutMilliseconds = 30000,                        // 요청 타임아웃 설정 (기본값: 30000ms, 없으면 생략 가능)
    QueryParameters = new Dictionary<string, string>    // 쿼리 파라미터 설정 (없으면 생략 가능)
    {
        ["Type"] = "Search"
    }
};
```

추가적으로 `APIRequest.Create()` 메서드를 사용해서도 요청 생성이 가능합니다.

### 요청 파라미터 추가

인터페이스(`IAPIRequest`)를 사용하지 않고, 구체 클래스(`APIRequest`)를 사용할 경우 메서드 체이닝을 통해 요청 파라미터를 추가할 수 있습니다.

```csharp
request
    .AddQueryParameter("page", "1")
    .AddPathParameter("id", "123")
    .AddHeader("Custom-Header", "value")
    .WithTimeout(5000)
    .WithRetryCount(3);
```

### 요청 본문 설정

#### JSON 요청
```csharp
request.Body = RequestBody.CreateJson(new { name = "test" });
```

#### Form Data 요청
```csharp
var formData = new Dictionary<string, string>
{
    ["field1"] = "value1",
    ["field2"] = "value2"
};
request.Body = RequestBody.CreateFormData(formData);
```

#### URL Encoded 요청
```csharp
request.Body = RequestBody.CreateUrlEncoded(formData);
```

## API 응답 처리

```csharp
var response = await api.SendAsync<ResponseType>(request);

if (response.IsSuccess)
{
    var data = response.Data;
    // 데이터 처리
}
else
{
    var error = response.ErrorMessage;
    // 에러 처리
}
```

## 재시도 정책

기본 재시도 정책을 사용하거나 커스텀 정책을 구성할 수 있습니다.

```csharp
var retryPolicy = RetryPolicy.Create(
    maxRetryCount: 3,
    delayStrategy: retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
    shouldRetry: ex => ex is HttpRequestException,
    onRetry: (ex, count) => Console.WriteLine($"Retry {count}: {ex.Message}")
);

var api = new CoreAPIClient(apiSetup, retryPolicy: retryPolicy);
```

## 설정 관리

### API 설정 컨테이너 사용

설정 컨테이너를 통해 여러 API 설정을 관리할 수 있습니다.

```csharp
var setupFiles = new Dictionary<string, IIOHelper>
{
    ["API1"] = new RegistryHelper("path1"),
    ["API2"] = new RegistryHelper("path2")
};

var container = new APISetupContainer(setupFiles);
var setup = container.GetSetup("API1");
```

### 설정 업데이트

컨테이너를 통해 API 설정을 업데이트 하거나 `APIConnectionInfo`를 통해 직접 설정을 업데이트할 수 있습니다.

```csharp
container.UpdateSetup("API1", new APIConnectionInfo
{
    BaseUrl = "https://new-api.example.com",
    Endpoints = new Dictionary<string, string>
    {
        ["NewEndpoint"] = "api/new"
    }
});
```

```csharp
var connectionInfo = new APIConnectionInfo
{
    BaseUrl = "https://api.example.com",    // API Base URL (포트를 포함한 전체 공통 URL)
    Endpoints = new Dictionary<string, string>  // API 엔드포인트 설정 (실제 API 경로)
    {
        ["GetData"] = "api/data",
        ["CreateItem"] = "api/items",
        ["UpdateItem"] = "api/items/{id}"
    }
};

var apiSetup = new APISetup(ioHelper, "API_NAME");  // API 설정 생성 (IOHelper를 통해 설정 입/출력)
apiSetup.UpdateConnectionInfo(connectionInfo);  // API 설정 저장/업데이트
```

## 사용 예시

실제 API 호출 예시

```csharp
// TEST 프로젝트
public class APICallTest
{
    private IIOHelper _ioHelper;
    private string SectionName = "SECUiDEA_API";
    private string WebSectionName = "Web_API";
        
    public APICallTest()
    {
        _ioHelper = new RegistryHelper("SoftWare\\SECUiDEA\\APITest");
        SetupTestAPI();
    }

    // API 설정
    public void SetupTestAPI()
    {
        var connectionInfo = new APIConnectionInfo
        {
            BaseUrl = "http://192.168.0.63:2763",
            Endpoints = new Dictionary<string, string>
            {
                ["GetTokenSECU"] = "/CommonAPIv1/GetTokenSECU",
                ["GetVisitInfoList"] = "/CommonAPIv1/GetVisitInfoList",
                ["SetVisitReserveInfo"] = "/CommonAPIv1/SetVisitReserveInfo"
            }
        };
            
        var apiSetup = new APISetup(_ioHelper, SectionName);
        apiSetup.UpdateConnectionInfo(connectionInfo);

        var connectionInfo2 = new APIConnectionInfo
        {
            BaseUrl = "http://192.168.0.63:2751",
            Endpoints = new Dictionary<string, string>
            {
                ["GetVisitInfoList"] = "/Visit/GetVisitInfoList"
            }
        };
            
        var webApiSetup = new APISetup(_ioHelper, WebSectionName);
        webApiSetup.UpdateConnectionInfo(connectionInfo2);
    }
    
    // GetToken 테스트 (GetAPIKey() 함수에서 상세 구현)
    [Fact]
    public async Task GetTokenTest()
    {
        // Act
        var response = await GetAPIKey();
            
        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.ErrorCode == "0000");
    }
    
    // VisitInfoList 조회 테스트
    [Fact]
    public async Task GetVisitInfoListTest()
    {
        // Arrange
        var response = await GetAPIKey();
        var token = response.Data.Token;
            
        IAPISetup apiSetup = new APISetup(_ioHelper, SectionName);
        ICoreAPI api = new CoreAPIClient(apiSetup);

        VisitInfoModel model = new VisitInfoModel
        {
            VisitStatusID = "1",
            VisitSDate = "2025-01-01",
            VisitEDate = "2025-01-31"
        };
            
        Dictionary<string, string> formModel = new Dictionary<string, string>
        {
            ["Type"] = "Search",
            ["VisitStatusID"] = model.VisitStatusID,
            ["VisitSDate"] = model.VisitSDate,
            ["VisitEDate"] = model.VisitEDate
        };
            
        IAPIRequest requestParam = new APIRequest
        {
            Authentication = new BearerTokenAuthentication(token),
            Body = RequestBody.CreateFormData(formModel)
        };
            
        // Act
        var visitInfoList = await api.PostAsync<object>("GetVisitInfoList", requestParam);
            
        // Assert
        Assert.True(visitInfoList.IsSuccess);
        Assert.NotNull(visitInfoList.Data);
    }

    // VisitInfoList 조회 테스트 (API 두 개 이상 설정 테스트)
    [Fact]
    public async Task GetVisitInfoListWebTest()
    {
        // Arrange
        var response = await GetAPIKey();
        var token = response.Data.Token;
            
        IAPISetup apiSetup = new APISetup(_ioHelper, WebSectionName);
        ICoreAPI api = new CoreAPIClient(apiSetup);

        VisitInfo model = new VisitInfo
        {
            Type = "VisitSearch",
            Status = ["0", "1", "2", "3"],
            VisitSDate = "2025-01-01",
            VisitEDate = "2025-01-31",
        };
            
        IAPIRequest request = new APIRequest
        {
            Authentication = new BearerTokenAuthentication(token),
            Body = RequestBody.CreateJson(model)
        };
            
        // Act
        var visitInfoList = await api.PostAsync<object>("GetVisitInfoList", request);

        JObject data = visitInfoList.Data as JObject;
            
        // Assert
        Assert.True(visitInfoList.IsSuccess);
        Assert.NotNull(visitInfoList.Data);
        Assert.True(data?.ContainsKey("success"));
        Assert.Equal(data.GetValue("success"), true);
    }
    
    // API Key 조회
    private async Task<IAPIResponse<TokenResponseModel>> GetAPIKey()
    {
        // Arrange
        IAPISetup apiSetup = new APISetup(_ioHelper, SectionName);
        ICoreAPI api = new CoreAPIClient(apiSetup);
            
        Dictionary<string, string> formModel = new Dictionary<string, string>
        {
            ["ID"] = "apiuser",
            ["Email"] = "api@api.com"
        };

        IAPIRequest requestParam = new APIRequest
        {
            Body = RequestBody.CreateFormData(formModel)
        };

        // Act
        return await api.PostAsync<TokenResponseModel>("GetTokenSECU", requestParam);
    }
}
```