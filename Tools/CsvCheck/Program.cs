using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

// Standalone harness for the Contacts CSV import/export (ContactsPage ->
// Import CSV / Export CSV). ContactCsvService is a pure static with no MAUI
// dependencies, so it links straight into a console project; the model is a
// local stand-in (StandaloneContact.cs) with the same field shape, so the
// round-trip behaviour tested here is the behaviour that ships.
var failures = new List<string>();
void Check(bool condition, string name)
{
    Console.WriteLine((condition ? "PASS " : "FAIL ") + name);
    if (!condition) failures.Add(name);
}

// 1. Build -> Parse round trip, including commas/quotes/newlines in fields.
var originals = new List<EmailContact>
{
    new()
    {
        Email = "alice@example.com",
        Name = "Alice \"The Builder\", Jr.",
        Channel = "Alice Builds",
        VideoTitle = "Live-edge table, part 1",
        VideoDescription = "Line one.\nLine two, with a comma.",
        VideoUrl = "https://youtube.com/watch?v=abc",
        ExtractedAt = new DateTime(2026, 9, 1, 10, 30, 0, DateTimeKind.Utc),
        IsBlocked = true,
        LastEmailed = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc),
    },
    new() { Email = "bob@example.com", Name = "Bob" },
};

var csv = ContactCsvService.Build(originals);
Check(csv.StartsWith("﻿"), "build emits UTF-8 BOM for Excel");
Check(csv.Contains("\"Alice \"\"The Builder\"\", Jr.\""), "build quotes commas+quotes (RFC 4180)");

var (parsed, skipped) = ContactCsvService.Parse(csv);
Check(skipped == 0, "round trip skips nothing");
Check(parsed.Count == 2, "round trip returns both contacts");
Check(parsed[0].Name == "Alice \"The Builder\", Jr.", "quoted name survives round trip");
Check(parsed[0].VideoDescription == "Line one.\nLine two, with a comma.", "multiline field survives round trip");
Check(parsed[0].IsBlocked, "blocked flag survives round trip");
Check(parsed[0].ExtractedAt.ToUniversalTime() == new DateTime(2026, 9, 1, 10, 30, 0, DateTimeKind.Utc), "extracted date survives round trip");
Check(parsed[1].Email == "bob@example.com" && parsed[1].Name == "Bob", "minimal contact survives round trip");

// 2. Header aliases: a spreadsheet-style header maps onto the same fields.
var aliasCsv = "Full Name,Email Address,Channel Name,Title,URL,Blocked\r\n"
    + "Carol,carol@example.com,Carol Crafts,My video,https://example.com/v,yes\r\n";
var (aliasParsed, aliasSkipped) = ContactCsvService.Parse(aliasCsv);
Check(aliasSkipped == 0 && aliasParsed.Count == 1, "alias header parses one contact");
Check(aliasParsed[0].Name == "Carol", "alias: Full Name -> name");
Check(aliasParsed[0].Channel == "Carol Crafts", "alias: Channel Name -> channel");
Check(aliasParsed[0].VideoTitle == "My video", "alias: Title -> video title");
Check(aliasParsed[0].IsBlocked, "alias: Blocked=yes -> true");

// 3. Junk rows are skipped, not imported.
var junkCsv = "email,name\r\n"
    + "good@example.com,Good\r\n"
    + "not-an-email,Bad\r\n"
    + ",Empty\r\n"
    + "\r\n"
    + "alsobad@,Bad2\r\n"
    + "second@example.com,Second\r\n";
var (junkParsed, junkSkipped) = ContactCsvService.Parse(junkCsv);
Check(junkParsed.Count == 2, "junk: two valid contacts parsed");
Check(junkSkipped == 3, "junk: three bad rows skipped (blank line ignored)");
Check(junkParsed[0].Email == "good@example.com" && junkParsed[1].Email == "second@example.com", "junk: valid rows keep order");

// 4. Unusable files raise a user-showable FormatException, never garbage.
foreach (var (label, bad) in new[] { ("empty", ""), ("blank", "\r\n\r\n"), ("no-header", "foo,bar\n1,2\n") })
{
    try
    {
        ContactCsvService.Parse(bad);
        Check(false, $"bad file raises: {label}");
    }
    catch (FormatException ex)
    {
        Check(ex.Message.Length > 10, $"bad file raises FormatException with helpful text: {label}");
    }
}

Console.WriteLine();
Console.WriteLine(failures.Count == 0 ? "ALL CHECKS PASSED" : $"{failures.Count} CHECK(S) FAILED");
return failures.Count == 0 ? 0 : 1;
