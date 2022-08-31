using System.ComponentModel;
using System.Reflection;

namespace fh_service_directory_api.core;

public static class Utility
{

    public static string GetEnumDescription(Enum value)
    {
        FieldInfo? fi = value.GetType().GetField(value.ToString());

        if (fi != null && fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}
