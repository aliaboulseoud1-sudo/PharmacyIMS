namespace PharmacyIMS.Controllers
{
    public class AiAssistantController : Controller
    {
        private readonly IAiAssistantService _aiAssistantService;

        public AiAssistantController(IAiAssistantService aiAssistantService)
        {
            _aiAssistantService = aiAssistantService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = "الرجاء كتابة سؤال قبل الإرسال."
                });
            }

            try
            {
                var reply = await _aiAssistantService.AskAsync(request.Message.Trim(), request.History, cancellationToken);

                return Json(new ChatResponseDto
                {
                    Success = true,
                    Reply = reply
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Gemini Error]: {ex.ToString()}");

                return Json(new ChatResponseDto
                {
                    Success = false
                    //ErrorMessage = $"خطأ تفصيلي: {ex.Message}"
                });
            }
        }
    }
}