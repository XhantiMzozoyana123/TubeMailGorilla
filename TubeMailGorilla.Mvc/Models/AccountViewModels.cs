using System.ComponentModel.DataAnnotations;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Domain;

namespace TubeMailGorilla.Mvc.Models;

/// <summary>Login form.</summary>
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

/// <summary>Registration form.</summary>
public class RegisterViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>The public pricing page model (plan catalog from configuration).</summary>
public class PricingViewModel
{
    public List<SubscriptionPlanDefinition> Plans { get; set; } = new();
    public int FreeLeadsPerExtraction { get; set; }
    public int FreeContactsVisible { get; set; }
    public int FreeEmailsPerCampaign { get; set; }
}

/// <summary>Mirror of the API's EntitlementsDto for the subscription page.</summary>
public class EntitlementsViewModel
{
    public bool IsSubscribed { get; set; }
    public string PlanId { get; set; } = "free";
    public string PlanName { get; set; } = "Free";
    public int MaxLeadsPerExtraction { get; set; }
    public int MaxContactsVisible { get; set; }
    public int MaxEmailsPerCampaign { get; set; }
    public bool IcebreakerEnabled { get; set; }
    public bool EmailTemplatesEnabled { get; set; }
    public bool BlocklistEnabled { get; set; }
}

/// <summary>The signed-in user's subscription dashboard model.</summary>
public class SubscriptionIndexViewModel
{
    public SubscriptionIndexViewModel(SubscriptionStatusResponse status, EntitlementsViewModel entitlements)
    {
        Status = status;
        Entitlements = entitlements;
    }

    public SubscriptionStatusResponse Status { get; }
    public EntitlementsViewModel Entitlements { get; }
}
