namespace TubeMailGorilla.Mvc.Tests;

using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;
using Xunit;

/// <summary>
/// Guards the snapshot backfill in <c>AddContactAsync</c>: a lead that already
/// exists must gain its video URL and snapshots when a later extraction
/// re-encounters it. Without that, the frames captured on a repeat run are
/// silently discarded and the lead can never be backfilled.
/// </summary>
public class SnapBackfillTests
{
    private static string NewDbPath() =>
        Path.Combine(Path.GetTempPath(), "tmg_test_" + Guid.NewGuid().ToString("N") + ".db3");

    [Fact]
    public async Task DuplicateLead_GainsSnapshotsOnRepeatExtraction()
    {
        var path = NewDbPath();
        var db = new DatabaseService(path);
        try
        {
            // A lead stored before the snapshot feature existed.
            var existing = new EmailContact
            {
                Email = "old@lead.com",
                Name = "Old Lead",
                ExtractedAt = DateTime.Now
            };
            Assert.Equal(1, await db.AddContactAsync(existing));

            // A repeat extraction of the same creator, now carrying snapshots.
            // Note the padded / mixed-case email: it must still match.
            var rediscovered = new EmailContact
            {
                Email = "  Old@Lead.com  ",
                Name = "Old Lead",
                VideoUrl = "https://www.youtube.com/watch?v=abc123",
                VideoSnapshot = new List<string> { "frame1", "frame2" },
                VideoSnapshotTimestamps = new List<double> { 11, 21 },
                ExtractedAt = DateTime.Now
            };

            // 0 = duplicate, so the caller does not count it as a new lead.
            Assert.Equal(0, await db.AddContactAsync(rediscovered));

            var stored = await db.GetContactAsync(existing.Id);
            Assert.NotNull(stored);
            Assert.Equal("https://www.youtube.com/watch?v=abc123", stored!.VideoUrl);
            Assert.Equal(2, stored.VideoSnapshot.Count);
            Assert.Equal(new[] { 11d, 21d }, stored.VideoSnapshotTimestamps);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task DuplicateLead_KeepsExistingSnapshotsWhenNewRunHasNone()
    {
        var path = NewDbPath();
        var db = new DatabaseService(path);
        try
        {
            var first = new EmailContact
            {
                Email = "keep@lead.com",
                VideoUrl = "https://www.youtube.com/watch?v=xyz",
                VideoSnapshot = new List<string> { "original" },
                VideoSnapshotTimestamps = new List<double> { 10 },
                ExtractedAt = DateTime.Now
            };
            Assert.Equal(1, await db.AddContactAsync(first));

            // A later run where the video could not be downloaded this time.
            var second = new EmailContact
            {
                Email = "keep@lead.com",
                ExtractedAt = DateTime.Now
            };
            Assert.Equal(0, await db.AddContactAsync(second));

            var stored = await db.GetContactAsync(first.Id);
            Assert.Equal(new List<string> { "original" }, stored!.VideoSnapshot);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task NewLead_ReportsAsInserted()
    {
        var path = NewDbPath();
        var db = new DatabaseService(path);
        try
        {
            var contact = new EmailContact
            {
                Email = "Fresh@Lead.com",
                VideoUrl = "https://www.youtube.com/watch?v=new",
                VideoSnapshot = new List<string> { "a" },
                VideoSnapshotTimestamps = new List<double> { 1 },
                ExtractedAt = DateTime.Now
            };
            Assert.Equal(1, await db.AddContactAsync(contact));
            Assert.True(contact.Id > 0);

            var stored = await db.GetContactAsync(contact.Id);
            Assert.Equal("fresh@lead.com", stored!.Email);
            Assert.Single(stored.VideoSnapshot);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
