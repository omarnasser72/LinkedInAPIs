namespace LinkedInAPIs
{
    using System.Text.Json.Serialization;

    public class ShareCommentary
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = null!;
    }

    public class ShareContent
    {
        [JsonPropertyName("shareCommentary")]
        public ShareCommentary ShareCommentary { get; set; } = null!;

        [JsonPropertyName("shareMediaCategory")]
        public string ShareMediaCategory { get; set; } = null!;
    }

    public class SpecificContent
    {
        [JsonPropertyName("com.linkedin.ugc.ShareContent")]
        public ShareContent ShareContent { get; set; } = null!;
    }

    public class Visibility
    {
        [JsonPropertyName("com.linkedin.ugc.MemberNetworkVisibility")]
        public string MemberNetworkVisibility { get; set; } = null!;
    }

    public class LinkedInPostPayload
    {
        [JsonPropertyName("author")]
        public string Author { get; set; } = null!;

        [JsonPropertyName("lifecycleState")]
        public string LifecycleState { get; set; } = null!;

        [JsonPropertyName("specificContent")]
        public SpecificContent SpecificContent { get; set; } = null!;

        [JsonPropertyName("visibility")]
        public Visibility Visibility { get; set; } = null!;
    }

    public class MessageModel
    {
        public string Message { get; set; } = null!;
    }
}
