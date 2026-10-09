using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

// Live end-to-end validation of the icebreaker path against the REAL local
// Ollama server, using the app's real LLMService + AIService from the built
// DLL. Mirrors the app's bin appsettings (qwen3-vl:4b, temp 0.2, ctx 8192,
// 300s budget, keep_alive 60).
var fail = 0;
void Check(string name, bool ok, string detail = "")
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}{(detail.Length > 0 ? " -> " + detail : "")}");
    if (!ok) fail++;
}

var settings = new LlmSettings();
// Mirror the app's appsettings.json: TEXT work on llama3 (qwen3-vl's thinking
// trace can't be disabled on Ollama 0.40 and eats custom-prompt budgets),
// vision stays on qwen3-vl:4b (not exercised here - text only).
settings.OllamaModel = "llama3:latest";
using var llm = new LLMService(settings);
var ai = new AIService(llm); // NullImageGenerationService fallback - text only here

var contact = new EmailContact
{
    Name = "John",
    Channel = "Woodworking Masters",
    VideoTitle = "How I built a live-edge table"
};

// --- 1) empty-response path surfaces a real LastError (num_predict=1 forces
//         the thinking trace to eat the whole budget -> empty content). ---
var tiny = await llm.GenerateTextAsync("hi", maxTokens: 1);
Check("empty response -> 'LLM Error: No response generated.'",
    tiny.StartsWith("LLM Error: No response"), tiny);
Check("empty response -> LastError is set", !string.IsNullOrWhiteSpace(llm.LastError),
    llm.LastError ?? "(null)");

// --- 2) the user's failing path: CUSTOM-instruction icebreaker ---
var custom = await ai.GenerateIcebreakerAsync(contact,
    "Open with a pun about sawdust and include the word 'sheesh'.");
Check("custom icebreaker generated", !string.IsNullOrWhiteSpace(custom), custom ?? "(null)");
Check("custom success -> LastError cleared", llm.LastError is null, llm.LastError ?? "(null)");

// --- 3) the standard icebreaker button ---
var standard = await ai.GenerateIcebreakerAsync(contact, string.Empty);
Check("standard icebreaker generated", !string.IsNullOrWhiteSpace(standard), standard ?? "(null)");

Console.WriteLine(fail == 0 ? "ALL CHECKS PASSED" : $"{fail} CHECK(S) FAILED");
Environment.Exit(fail == 0 ? 0 : 1);
