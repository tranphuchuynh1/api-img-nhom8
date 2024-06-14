using Microsoft.AspNetCore.Identity;

namespace APIImage.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
