namespace FamilyHubs.SharedKernel.Identity
{
    public static class FamilyHubsClaimTypes
    {
        public const string Role = "Role";
        public const string OrganisationId = "OrganisationId";
        public const string AccountStatus = "AccountStatus";
        public const string LoginTime = "LoginTime";
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string PhoneNumber = "PhoneNumber ";
    }

    public static class RoleTypes
    {
        public const string DfeAdmin = "DfeAdmin";
        public const string LaAdmin = "LaAdmin";
        public const string VcsAdmin = "VcsAdmin";
        public const string Professional = "Professional";
    }

    internal static class AuthenticationConstants
    {
        internal const string BearerToken = "BearerToken";
        internal const string IdToken = "id_token";


        internal const string AccountPaths = "Account/";

        /// <summary>
        /// This is the path called from the browser to trigger the sign-out process
        /// </summary>
        internal const string SignOutPath = "/Account/signout";

        /// <summary>
        /// This is the path one-login will return to after logout. The oidc library will then catch this, perform some tasks then 
        /// redirect to the path specified in the app settings
        /// </summary>
        internal const string AccountLogoutCallback = "/Account/logout-callback";  
    }

    internal static class StubConstants
    {
        internal const string LoginPagePath = "/account/stub/loginpage/";
        internal const string RoleSelectedPath = "/account/stub/roleSelected";
    }
}
