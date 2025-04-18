# LinkedIn Post Sharing API Reference Document

## 1. Project Overview

This **ASP.NET Core Web API** project integrates with **LinkedIn's APIs** to authenticate a user and share a post on their behalf. The project uses **dependency injection** for configuration and an `HttpClient` for making HTTP calls to LinkedIn endpoints.

### Key Features

- **SignIn Functionality**: Retrieves a user’s unique identifier (`Sub` property) by calling the SignIn endpoint.
- **Share Post**: Constructs a payload using the retrieved `Sub` and the message provided by the client, then calls LinkedIn’s Share API.
- **Custom JSON Serialization**: Uses `JsonPropertyName` attributes in model classes to ensure JSON keys match LinkedIn’s requirements (e.g., keys with periods).

---

## 2. Project Setup

### Prerequisites

- **LinkedIn Developers Account**: Create an account at [LinkedIn Developers](https://www.linkedin.com/developers/).
- **LinkedIn Developers App**: Create an app and associate it with your company page on LinkedIn.
- **LinkedIn Developers Verification**: Verify the app as required by LinkedIn.
- **LinkedIn OAuth2**: Generate an OAuth2 access token which will be used for sign-in.

### Dependencies & Configuration

- **Dependency Injection**: Injects `IConfiguration` and a singleton `HttpClient` instance into the controller.
- **AppSettings**: OAuth2 token stored in `appsettings.json` and accessed using `_configuration["Oauth2AccessToken"]`.

### Controller Registration

The controller is decorated with:

```csharp
[Route("api/[controller]")]
[ApiController]
```

This sets up route mapping and enables model validation.

---

## 3. Controller Walkthrough

### Constructor & Dependency Injection

```csharp
public LinkedInController(IConfiguration configuration, HttpClient httpClient)
{
    _configuration = configuration;
    _httpClient = httpClient;
}
```

- **Purpose**: Injects configuration and HttpClient.
- **Benefits**: Simplifies access to config values and reuses HttpClient instance.

---

### SignIn Method

```csharp
private async Task<SigninDTO?> SignIn()
{
    var token = _configuration["Oauth2AccessToken"];
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _httpClient.GetAsync(Constants.SignInWithLinkedInUsingOpenIDConnect);
    var result = await response.Content.ReadFromJsonAsync<SigninDTO>();
    return result;
}
```

#### Explanation

- **Access Token Retrieval**: Gets token from config.
- **Authorization Header**: Sets the `Bearer` token.
- **API Call**: Uses `Constants.SignInWithLinkedInUsingOpenIDConnect` to retrieve user info.
- **Deserialization**: Maps response to `SigninDTO`, extracting the `Sub` (LinkedIn user ID).

---

### SharePost Method

```csharp
[HttpPost]
public async Task<JsonResult> SharePost([FromBody] MessageModel messageModel)
{
    var Sub = (await SignIn())?.Sub;

    var payload = new LinkedInPostPayload
    {
        Author = "urn:li:person:" + Sub,
        LifecycleState = "PUBLISHED",
        SpecificContent = new SpecificContent
        {
            ShareContent = new ShareContent
            {
                ShareCommentary = new ShareCommentary { Text = messageModel.Message },
                ShareMediaCategory = "NONE"
            }
        },
        Visibility = new Visibility
        {
            MemberNetworkVisibility = "PUBLIC"
        }
    };

    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var json = JsonSerializer.Serialize(payload, jsonOptions);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync(Constants.ShareOnLinkedIn, content);

    var jsonResult = (response.IsSuccessStatusCode)
        ? new JsonResult(new { Success = true, Message = "Post shared successfully." })
        : new JsonResult(new { Success = false, Message = "Failed to share post." });

    return jsonResult;
}
```

#### Explanation

- **Retrieve `Sub` Property**: Uses `SignIn()` to get the LinkedIn user ID.
- **Payload Construction**: Builds `LinkedInPostPayload` with properly named JSON fields.
- **Serialization**: Applies camelCase naming policy.
- **POST Request**: Sends the JSON payload to LinkedIn's Share API. Returns success/failure message.

---

## 4. Models & JSON Property Mapping

To match LinkedIn's expected JSON schema, model classes use `[JsonPropertyName]`.

```csharp
using System.Text.Json.Serialization;

public class ShareCommentary
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class ShareContent
{
    [JsonPropertyName("shareCommentary")]
    public ShareCommentary ShareCommentary { get; set; }

    [JsonPropertyName("shareMediaCategory")]
    public string ShareMediaCategory { get; set; }
}

public class SpecificContent
{
    [JsonPropertyName("com.linkedin.ugc.ShareContent")]
    public ShareContent ShareContent { get; set; }
}

public class Visibility
{
    [JsonPropertyName("com.linkedin.ugc.MemberNetworkVisibility")]
    public string MemberNetworkVisibility { get; set; }
}

public class LinkedInPostPayload
{
    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("lifecycleState")]
    public string LifecycleState { get; set; }

    [JsonPropertyName("specificContent")]
    public SpecificContent SpecificContent { get; set; }

    [JsonPropertyName("visibility")]
    public Visibility Visibility { get; set; }
}
```

### Key Points

- **Custom JSON Keys**: `[JsonPropertyName]` ensures exact match with LinkedIn’s required JSON structure.
- **Model Organization**: Nested structure improves clarity and manageability of payload components.

---

Let me know if you'd like me to generate this into an actual `README.md` file!
