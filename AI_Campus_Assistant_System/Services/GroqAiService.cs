using System.Text;
using System.Text.Json;

namespace AI_Campus_Assistant.Services
{
    /// <summary>
    /// AI Service using Groq API (Llama 3.3 70B).
    /// Free API key at: https://console.groq.com
    /// </summary>
    public class GroqAiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private const string MODEL   = "llama-3.3-70b-versatile";

        public GroqAiService(IConfiguration config, IHttpClientFactory factory)
        {
            _http   = factory.CreateClient();
            _apiKey = config["GroqApiKey"] ?? "";
        }

        public async Task<string> AskAsync(string question, string context = "")
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "AI service not configured. Please add GroqApiKey to appsettings.json.\n" +
                       "Get a free key at: https://console.groq.com";

            var systemPrompt =
                "You are an AI assistant for a Smart University Campus system called \"AI Campus\". " +
                "You help students, teachers and admins with academic queries, campus information, " +
                "and general university-related questions. Be concise, helpful and friendly. " +
                "Always respond in the same language the user uses (Urdu or English).";

            var userMessage = string.IsNullOrEmpty(context)
                ? question
                : $"Context: {context}\n\nUser Question: {question}";

            var payload = new
            {
                model      = MODEL,
                max_tokens = 1024,
                messages   = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userMessage  }
                }
            };

            try
            {
                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _http.PostAsync(API_URL, content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return $"AI service error: {response.StatusCode}. Check your GroqApiKey.\nDetails: {err}";
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                using var doc  = JsonDocument.Parse(resultJson);

                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response from AI.";
            }
            catch (Exception ex)
            {
                return $"AI Error: {ex.Message}";
            }
        }

        public async Task<string> GetTeachingSuggestionsAsync(
            string courseName, int totalStudents, double avgAttendance, double avgGrade)
        {
            var prompt = $"""
                As an educational AI, provide 3-4 specific teaching improvement suggestions for:
                Course: {courseName}
                Total Students: {totalStudents}
                Average Attendance: {avgAttendance:F1}%
                Average Grade: {avgGrade:F1}/100

                Focus on practical, actionable suggestions for the teacher.
                Format as numbered list. Be concise (max 150 words).
                """;

            return await AskAsync(prompt);
        }
    }
}
