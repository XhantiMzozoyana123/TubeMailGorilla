using System.Text.Json.Serialization;

namespace TubeMailGorilla.Maui.Models;

public class Emailer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Job { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoDescription { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoTranscript { get; set; } = string.Empty;
    public string SearchTerm { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public enum EmailerStatus
{
    New,
    Contacted,
    Interested,
    NotInterested,
    FollowUp
}