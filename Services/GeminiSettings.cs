namespace PharmacyIMS.Services
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = "";

        public string Model { get; set; } = "gemini-3.8-flash";

        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";

        public double Temperature { get; set; } = 0.3;

        public int MaxOutputTokens { get; set; } = 800;
    }
}
