namespace PharmacyIMS.Services
{
    public class GeminiAiService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly IPharmacyContextService _contextService;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiAiService> _logger;

        private const string SystemPersona =
            "أنت \"المساعد الذكي\" لنظام إدارة مخزون صيدلية (PharmacyIMS). " +
            "مهمتك الإجابة على أسئلة صاحب الصيدلية أو الموظفين حول المخزون والمنتجات والموردين والمبيعات والمشتريات، " +
            "بالاعتماد حصرياً على البيانات الفعلية المرفقة أدناه ضمن قسم \"بيانات النظام الحالية\". " +
            "لا تخترع أرقاماً أو أسماء منتجات أو موردين غير موجودة في هذه البيانات. " +
            "إذا لم تجد إجابة كافية ضمن البيانات المتاحة، أخبر المستخدم بوضوح أن المعلومة غير متوفرة حالياً في النظام بدلاً من التخمين. " +
            "أجب دائماً باللغة العربية، بأسلوب مهني ومختصر ومباشر، واستخدم نقاط أو جداول نصية عند تعداد عدة أصناف.";

        public GeminiAiService(
            HttpClient httpClient,
            IPharmacyContextService contextService,
            IOptions<GeminiSettings> settings,
            ILogger<GeminiAiService> logger)
        {
            _httpClient = httpClient;
            _contextService = contextService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> AskAsync(string userMessage, List<ChatTurnDto>? history = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogWarning("Gemini API key is not configured.");
                return "لم يتم إعداد مفتاح واجهة الذكاء الاصطناعي (API Key) بعد. يرجى مراجعة إعدادات النظام مع المسؤول التقني.";
            }

            try
            {
                var contextSummary = await _contextService.BuildContextSummaryAsync();
                var systemInstruction = $"{SystemPersona}\n\n=== بيانات النظام الحالية ===\n{contextSummary}";

                var contents = new List<object>();

                if (history != null)
                {
                    foreach (var turn in history.TakeLast(8))
                    {
                        var role = turn.Role == "model" ? "model" : "user";
                        contents.Add(new
                        {
                            role,
                            parts = new[] { new { text = turn.Text } }
                        });
                    }
                }

                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                });

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemInstruction } }
                    },
                    contents,
                    generationConfig = new
                    {
                        temperature = _settings.Temperature,
                        maxOutputTokens = _settings.MaxOutputTokens
                    }
                };

                var url = $"{_settings.BaseUrl}/{_settings.Model}:generateContent?key={_settings.ApiKey}";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", _settings.ApiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, responseBody);
                    return "يشهد المساعد الذكي إقبالاً عالياً في الوقت الحالي. يرجى الانتظار بضع ثوانٍ وإعادة المحاولة.";

                    //return $"خطأ من سيرفر Gemini (Status {response.StatusCode}): {responseBody}";
                }

                using var doc = JsonDocument.Parse(responseBody);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Gemini API returned no candidates. Body: {Body}", responseBody);
                    return "لم يتمكن المساعد من توليد إجابة لهذا السؤال. حاول إعادة صياغته.";
                }

                var firstCandidate = candidates[0];
                var text = firstCandidate
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return string.IsNullOrWhiteSpace(text)
                    ? "لم يتمكن المساعد من توليد إجابة لهذا السؤال. حاول إعادة صياغته."
                    : text.Trim();
            }
            catch (TaskCanceledException)
            {
                return "استغرق الاتصال بالمساعد الذكي وقتاً طويلاً. يرجى المحاولة مرة أخرى.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling Gemini API.");
                return "عذراً، حدث خطأ غير متوقع أثناء معالجة سؤالك.";
            }
        }
    }
}
