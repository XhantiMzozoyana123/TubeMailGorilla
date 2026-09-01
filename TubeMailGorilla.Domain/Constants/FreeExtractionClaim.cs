namespace TubeMailGorilla.Domain.Constants;

/// <summary>
/// Identity claim used to enforce the free-plan quota of one extraction per
/// calendar month. A claim with this type and a value of "yyyy-MM" records that
/// the user has consumed their free extraction for that month. No schema change
/// is needed - it lives in the standard Identity AspNetUserClaims table.
/// </summary>
public static class FreeExtractionClaim
{
    public const string Type = "free_extraction_month";
}