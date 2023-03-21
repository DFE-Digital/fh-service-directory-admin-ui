namespace FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}
