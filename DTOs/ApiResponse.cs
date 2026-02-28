using System.Text.Json.Serialization;

namespace attendance_api.DTOs
{
    /// <summary>
    /// Standard API response wrapper.
    /// Property order = JSON output order: message → success → data → errors
    /// </summary>
    public class ApiResponse<T>
    {
        // ✅ Message sabse upar aayega JSON mein
        [JsonPropertyOrder(1)]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyOrder(2)]
        public bool Success { get; set; }

        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }
    }
}