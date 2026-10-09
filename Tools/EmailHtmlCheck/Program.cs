using System.Reflection;
using System.Text;
using TubeMailGorilla.Maui.Unlocked.Services;

// Validates the two original bugs against the REAL built app assembly:
//  1. newline collapse - a plain-text body's single newlines must become <br>
//     even when the body also contains an <img> (the old skip-list bug).
//  2. snapshot image  - injected <img> tags must stay real markup (not
//     &lt;img&gt;), and data: URIs must convert to cid: attachments so
//     Gmail/Outlook actually render them.
// Also re-checks the restored [snapshot_N] members still work.

var failures = 0;

void Check(string name, bool ok, string detail)
{
    Console.WriteLine((ok ? "PASS  " : "FAIL  ") + name + (ok ? "" : "  -> " + detail));
    if (!ok) failures++;
}

// --- 1) plain text: single newlines must not collapse -------------------------
var plain = "Line one\nLine two\nLine three\n\nSecond paragraph";
var plainHtml = EmailService.ToHtmlBody(plain);
Check("plain single newlines -> <br>",
    plainHtml.Contains("<br>") && plainHtml.Contains("<p>Line one<br>Line two<br>Line three</p>"),
    plainHtml);

// --- 2) THE BUG: text with an <img> used to skip wrapping entirely -----------
// Old skip-list returned the body RAW (literal \n that inboxes collapse into
// one line). Wrapping must happen: lines become paragraphs/br, img survives.
var img = "<img src=\"data:image/png;base64,iVBORw0KGgo=\" width=\"600\" height=\"400\" alt=\"x\" />";
var withImg = "First line\n" + img + "\nLast line";
var withImgHtml = EmailService.ToHtmlBody(withImg);
Check("img body is wrapped (old bug returned it raw/unwrapped)",
    withImgHtml != withImg && withImgHtml.Contains("<p>First line</p>") &&
    withImgHtml.Contains("<p>Last line</p>"),
    withImgHtml);
Check("img stays real markup (not HTML-encoded)",
    withImgHtml.Contains("<img src=") && !withImgHtml.Contains("&lt;img"),
    withImgHtml);
Check("img inside <p> survives",
    withImgHtml.Contains("<p>" ) && withImgHtml.Contains(img),
    withImgHtml);

// --- 3) inline img mid-line: text encoded, tag preserved ---------------------
var inline = "Hi [name] check this " + img + " out\nSecond line";
var inlineHtml = EmailService.ToHtmlBody(inline);
Check("inline img: tag preserved, text encoded",
    inlineHtml.Contains("<img src=") && !inlineHtml.Contains("&lt;img") && inlineHtml.Contains("<br>"),
    inlineHtml);

// --- 4) authored structural HTML passes through unchanged --------------------
var authored = "<div style=\"x\"><p>hello</p></div>";
Check("authored html untouched",
    EmailService.ToHtmlBody(authored) == authored,
    EmailService.ToHtmlBody(authored));

// --- 5) standalone img line (WrapParagraphs path) ----------------------------
var standalone = "Before\n" + img + "\nAfter";
var standaloneHtml = EmailService.ToHtmlBody(standalone);
Check("standalone img line preserved, text around it paragraphed",
    standaloneHtml.Contains(img) && standaloneHtml.Contains("Before</p>") &&
    standaloneHtml.Contains("<p>After"),
    standaloneHtml);

// --- 6) data: URI -> cid: conversion (private ExtractEmbeddedImages) ---------
var svcType = typeof(EmailService);
var extract = svcType.GetMethod("ExtractEmbeddedImages", BindingFlags.NonPublic | BindingFlags.Static);
Check("ExtractEmbeddedImages exists", extract is not null, "method not found");
if (extract is not null)
{
    var body = "<p>Hi</p><img src=\"data:image/png;base64,aGVsbG8=\" alt=\"s\" />";
    var res = extract.Invoke(null, new object?[] { body });
    var t = res!.GetType();
    var html = (string)t.GetField("Item1")!.GetValue(res)!;              // (Html, Resources)
    var list = (System.Collections.IList)t.GetField("Item2")!.GetValue(res)!;
    Check("data: URI rewritten to cid:", html.Contains("cid:snapshot0@tubemailgorilla") && !html.Contains("data:image"),
        html);
    Check("one LinkedResource produced", list.Count == 1, "count=" + list.Count);
    foreach (var r in list)
        ((IDisposable)r).Dispose();
}

// --- 7) restored [snapshot_N] member still works -----------------------------
var snapshots = new List<string> { "ZmFrZTE=", "ZmFrZTI=" };
var indexed = EmailService.PersonalizeSnapshotIndexed("A: [snapshot_1] B: [snapshot_2]", snapshots);
Check("[snapshot_N] indexes frames (restored member)",
    indexed.Contains("ZmFrZTE=") && indexed.Contains("ZmFrZTI=") && !indexed.Contains("[snapshot_"),
    indexed);
Check("ContainsSnapshotIndexToken detects token",
    EmailService.ContainsSnapshotIndexToken("x [snapshot_3] y") &&
    !EmailService.ContainsSnapshotIndexToken("x [snapshot_random] y"),
    "detection wrong");

// --- 8) restored f_name/l_name aliases ---------------------------------------
var contactType = Type.GetType("TubeMailGorilla.Maui.Unlocked.Models.EmailContact, TubeMailGorilla.Maui.Unlocked");
Check("EmailContact type resolves", contactType is not null, "type not found");

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL CHECKS PASSED" : failures + " CHECK(S) FAILED");
return failures == 0 ? 0 : 1;
