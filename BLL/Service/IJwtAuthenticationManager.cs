using DAL.Model;
using Microsoft.Extensions.Caching.Distributed;

namespace BLL.Service
{
    public interface IJwtAuthenticationManager
    {
        string Authenticate(AppUser appUser, string otp, IDistributedCache distributedCache);
    }
}
