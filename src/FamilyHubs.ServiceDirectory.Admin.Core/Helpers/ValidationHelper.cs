using PhoneNumbers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Helpers;

public static class ValidationHelper
{
    //https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    public static bool IsValidEmail(string email)
    {
        if (email.Length >= 255 && string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = new IdnMapping().GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        bool isValid = false;

        PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
        try
        {
            // throws if not a possible number somewhere in the world
            var parsedPhoneNumber = phoneNumberUtil.Parse(phoneNumber, "GB");
            // does more in depth validation, including if it's a valid UK number
            isValid = phoneNumberUtil.IsValidNumber(parsedPhoneNumber)
                      && phoneNumberUtil.GetRegionCodeForNumber(parsedPhoneNumber) == "GB"
                      // libphonenumber allows some characters that we don't want to allow
                      && !phoneNumber.Intersect("!\"£$%^&*={}'@~\\|?/").Any();
        }
        catch (NumberParseException)
        {
            // PhoneNumberUtil.Parse calls IsViablePhoneNumber(), which throws a NumberParseException if the number isn't a viable telephone number somewhere in the world
        }

        return isValid;
    }

    public static bool IsValidUrl(string url)
    {
        var pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        var rgx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return rgx.IsMatch(url);
    }
}