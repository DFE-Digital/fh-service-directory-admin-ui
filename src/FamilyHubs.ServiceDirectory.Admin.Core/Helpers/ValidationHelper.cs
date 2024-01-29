using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Tokens;

namespace FamilyHubs.ServiceDirectory.Admin.Core.Helpers
{
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

        public static bool IsValidPostcode(string postcode)
        {
            Regex SimpleValidUkPostcodeRegex = new(@"^\s*[a-z]{1,2}\d[a-z\d]?\s*\d[a-z]{2}\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex GdsAllowableCharsRegex = new(@"[-\(\)\.\[\]]+", RegexOptions.Compiled);

            if (postcode != null)
            {
                postcode = GdsAllowableCharsRegex.Replace(postcode, "");

                if (!SimpleValidUkPostcodeRegex.IsMatch(postcode))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
