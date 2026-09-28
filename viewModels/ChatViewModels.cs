using System.ComponentModel.DataAnnotations;

namespace PharmacyIMS.viewModels
{
    public class ChatTurnDto
    {
        public string Role { get; set; } = "user";
        public string Text { get; set; } = string.Empty;
    }

    public class ChatRequestDto
    {
        [Required]
        [StringLength(1000, ErrorMessage = "الرسالة طويلة جداً")]
        public string Message { get; set; } = string.Empty;

        public List<ChatTurnDto>? History { get; set; }
    }

    public class ChatResponseDto
    {
        public bool Success { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
