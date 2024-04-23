
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

    public static TEnum? ToOptionalEnum<TEnum>(string? urlString) where TEnum : struct, Enum
    {
        if (!Enum.TryParse(urlString, true, out TEnum enumValue))
        {
            return null;
        }

        return enumValue;
    }

    //todo: not enum extensions, extend string??
    //public static TEnum FromUrlString<TEnum>(string? urlString) where TEnum : struct, Enum
    //{
    //    ArgumentNullException.ThrowIfNullOrEmpty(urlString);

    //    if (!Enum.TryParse(urlString, true, out TEnum enumValue))
    //    {
    //        throw new InvalidOperationException($"Invalid {nameof(TEnum)} string representation: {urlString}");
    //    }

    //    return enumValue;
    //}

    //public static TEnum? FromOptionalUrlString<TEnum>(string? urlString) where TEnum : struct, Enum
    //{
    //    if (!Enum.TryParse(urlString, true, out TEnum enumValue))
    //    {
    //        return null;
    //    }

    //    return enumValue;
    //}
}

//public static class JourneyFlowExtensions
//{
//    public static string ToUrlString(this JourneyFlow flow)
//    {
//        return flow.ToString().ToLowerInvariant();
//    }

//    public static JourneyFlow FromUrlString(string? urlString)
//    {
//        //todo: have a default for when flow is not specified?
//        ArgumentNullException.ThrowIfNullOrEmpty(urlString);

//        if (!Enum.TryParse(urlString, true, out JourneyFlow flow))
//        {
//            //todo: throw here, or let consumer handle it?
//            throw new InvalidOperationException($"Invalid {nameof(JourneyFlow)} string representation: {urlString}");
//        }

//        return flow;
//    }

//    public static JourneyFlow? FromOptionalUrlString(string? urlString)
//    {
//        if (!Enum.TryParse(urlString, true, out JourneyFlow flow))
//        {
//            return null;
//        }

//        return flow;
//    }
//}