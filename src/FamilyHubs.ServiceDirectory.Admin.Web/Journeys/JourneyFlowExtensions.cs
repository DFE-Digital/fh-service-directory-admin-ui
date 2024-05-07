
namespace FamilyHubs.ServiceDirectory.Admin.Web.Journeys;

public static class EnumExtensions
{
    public static string ToUrlString<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        return enumValue.ToString().ToLowerInvariant();
    }

    public static TEnum ToEnum<TEnum>(this string? urlString) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNullOrEmpty(urlString);

        if (!Enum.TryParse(urlString, true, out TEnum enumValue))
        {
            throw new InvalidOperationException($"Invalid {nameof(TEnum)} string representation: {urlString}");
        }

        return enumValue;
    }

    public static TEnum? ToOptionalEnum<TEnum>(this string? urlString) where TEnum : struct, Enum
    {
        if (!Enum.TryParse(urlString, true, out TEnum enumValue))
        {
            return null;
        }

        return enumValue;
    }
}
