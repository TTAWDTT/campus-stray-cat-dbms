namespace CampusStrayCatSystem.Models;

// 业务编码统一放在 Models 层，接口和 SQL 都只保存这些英文编码；中文只负责展示。
public static class RoleCodes
{
    public const string Admin = "ADMIN";
    public const string Volunteer = "VOLUNTEER";
    public const string User = "USER";
    public const string Vet = "VET";

    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        { Admin, Volunteer, User, Vet };

    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class AreaTypes
{
    public const string Campus = "CAMPUS";
    public const string PublicArea = "PUBLIC_AREA";
    public const string ActivityArea = "ACTIVITY_AREA";
    public const string Greenbelt = "GREENBELT";
    public const string Gate = "GATE";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        { Campus, PublicArea, ActivityArea, Greenbelt, Gate };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
    public static string? Normalize(string? value) => Normalize(value, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["校区"] = Campus, ["公共区域"] = PublicArea, ["活动区域"] = ActivityArea
    });
    private static string? Normalize(string? value, IReadOnlyDictionary<string, string> aliases)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return aliases.TryGetValue(text, out var mapped) ? mapped : text.ToUpperInvariant();
    }
}

public static class RiskLevels
{
    public const string Low = "LOW";
    public const string Medium = "MEDIUM";
    public const string High = "HIGH";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Low, Medium, High };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim() switch { "低" => Low, "中" => Medium, "高" => High, var v => v.ToUpperInvariant() };
    }
}

public static class ServicePointTypes
{
    public const string Feeding = "FEEDING";
    public const string Nest = "NEST";
    public const string Activity = "ACTIVITY";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Feeding, Nest, Activity };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim() switch { "喂食点" or "投喂点" => Feeding, "猫窝" => Nest, "活动点" => Activity, var v => v.ToUpperInvariant() };
    }
}

public static class FacilityStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Maintenance = "MAINTENANCE";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Active, Inactive, Maintenance };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim() switch { "正常" or "NORMAL" => Active, "停用" or "INACTIVE" => Inactive, "需定期巡查" or "维护中" => Maintenance, var v => v.ToUpperInvariant() };
    }
}

public static class AnimalTypes
{
    public const string Cat = "CAT";
    public const string Dog = "DOG";
    public const string Other = "OTHER";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Cat, Dog, Other };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class EmergencyUrgencyLevels
{
    public const string Low = "LOW"; public const string Medium = "MEDIUM"; public const string High = "HIGH"; public const string Critical = "CRITICAL";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Low, Medium, High, Critical };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class EmergencyProcessStatuses
{
    public const string Submitted = "SUBMITTED"; public const string Assigned = "ASSIGNED"; public const string Processing = "PROCESSING";
    public const string Resolved = "RESOLVED"; public const string Closed = "CLOSED";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Submitted, Assigned, Processing, Resolved, Closed };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class MissingAlertStatuses
{
    public const string Processing = "PROCESSING"; public const string Found = "FOUND"; public const string Closed = "CLOSED";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Processing, Found, Closed };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class ReminderTypes
{
    public static readonly HashSet<string> Allowed = new(MedRecordTypes.Allowed, StringComparer.OrdinalIgnoreCase);
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class ReminderStatuses
{
    public const string Pending = "PENDING"; public const string Sent = "SENT"; public const string Completed = "COMPLETED";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { Pending, Sent, Completed };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class NestMaintenanceCodes
{
    public static readonly HashSet<string> MaterialTypes = new(StringComparer.OrdinalIgnoreCase) { "INSULATION_BOX", "FOOD_BOWL", "WATER_BOWL", "OTHER" };
    public static readonly HashSet<string> WeatherConditions = new(StringComparer.OrdinalIgnoreCase) { "SUNNY", "CLOUDY", "RAINY", "SNOWY", "OTHER" };
    public static readonly HashSet<string> DamageLevels = new(StringComparer.OrdinalIgnoreCase) { "NONE", "MINOR", "MAJOR" };
    public static readonly HashSet<string> ActionTypes = new(StringComparer.OrdinalIgnoreCase) { "CLEAN", "REPAIR", "REPLACE", "OTHER" };
}

public static class PaymentMethods
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { "ALIPAY", "WECHAT", "BANK_TRANSFER", "CASH", "OTHER" };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class FinanceRecordTypes
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { "FOOD", "MEDICAL", "SUPPLIES", "OTHER" };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class VisitTypes
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        { "INITIAL", "FOLLOW_UP", "FINAL" };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class BlacklistReasonTypes
{
    public const string Abandonment = "ABANDONMENT";
    public const string AnimalAbuse = "ANIMAL_ABUSE";
    public const string FalseInformation = "FALSE_INFORMATION";
    public const string Other = "OTHER";
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        { Abandonment, AnimalAbuse, FalseInformation, Other };
    public static bool IsValid(string? value) => value != null && Allowed.Contains(value.Trim());
}

public static class StatisticCodes
{
    public static readonly HashSet<string> MetricCodes = new(StringComparer.OrdinalIgnoreCase) { "TOTAL_DONATION", "TOTAL_EXPENSE", "NET_BALANCE", "DONATION_COUNT" };
    public static readonly HashSet<string> DimensionTypes = new(StringComparer.OrdinalIgnoreCase) { "PROJECT", "MONTH", "CAT" };
    public static readonly HashSet<string> Units = new(StringComparer.OrdinalIgnoreCase) { "CNY", "COUNT" };
}
