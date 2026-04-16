namespace Roxana_tema1.Middleware.Auth
{
    public class UserClaimModel
    {
        public Guid ClaimUserId {  get; set; }
        public IEnumerable<string> ClaimRoles { get; set; } = [];
    }
}
