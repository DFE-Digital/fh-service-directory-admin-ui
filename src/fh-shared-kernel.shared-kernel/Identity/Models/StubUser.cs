namespace FamilyHubs.SharedKernel.Identity.Models
{
    public class StubUser
    {
        public GovUkUser User { get; set; } = default!;
        public List<AccountClaim> Claims { get; set; } = default!;
    }
}
