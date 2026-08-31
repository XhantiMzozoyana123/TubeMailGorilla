using SQLite;

namespace TubeMailGorilla.Maui.Models;

/// <summary>
/// A user-editable message token mapped to a per-recipient data field.
/// Persisted in SQLite so users can add/edit/remove their own tokens on the
/// Settings page; every "[token]" found in an email subject/body is replaced
/// by the matching lead's value when sending.
/// </summary>
public class MessageParameter
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
}