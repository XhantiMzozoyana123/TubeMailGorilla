namespace TubeMailGorilla.Maui.Models;

/// <summary>Describes a subscription package available in the app.</summary>
public class SubscriptionPackage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}