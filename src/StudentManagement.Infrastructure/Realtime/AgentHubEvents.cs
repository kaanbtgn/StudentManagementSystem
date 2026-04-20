namespace StudentManagement.Infrastructure.Realtime;

public static class AgentHubEvents
{
    // --- OCR ---
    public const string OcrProgressUpdated = "OcrProgressUpdated";
    public const string OcrCompleted       = "OcrCompleted";
    public const string OcrFailed          = "OcrFailed";

    // --- Chat / Agent ---
    // Kullanıcının mesajı sunucuya ulaştı, işlem kuyruğa alındı
    public const string MessageReceived = "MessageReceived";

    // Agent mesajı aldı, düşünüyor/işliyor
    public const string AgentThinking = "AgentThinking";

    // Agent tam yanıtı hazırladı (streaming kullanılmadığında)
    public const string AgentResponseCompleted = "AgentResponseCompleted";

    // Agent veya iş akışında hata oluştu
    public const string AgentError = "AgentError";

    // Streaming token — Faz 13'te etkinleştirilecek
    public const string AgentTokenReceived = "AgentTokenReceived";
}
