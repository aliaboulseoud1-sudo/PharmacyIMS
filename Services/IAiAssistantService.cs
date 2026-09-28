using PharmacyIMS.viewModels;

namespace PharmacyIMS.Services
{
    public interface IAiAssistantService
    {
        Task<string> AskAsync(string userMessage, List<ChatTurnDto>? history = null, CancellationToken cancellationToken = default);
    }
}
