using LinkedInAPIs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LinkedInAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkedInController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LinkedInController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<SigninDTO?> SignIn()
        {

            var token = _configuration["Oauth2AccessToken"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(Constants.SignInWithLinkedInUsingOpenIDConnect);


            var result = response.Content.ReadFromJsonAsync<SigninDTO>().Result;

            return result;
        }


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
    }
}
