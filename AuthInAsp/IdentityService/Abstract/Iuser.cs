using AuthInAsp.Contracts.Generic;
using AuthInAsp.model;

namespace AuthInAsp.IdentityService.Abstract
{
    public interface Iuser : IGenericRepository<User_En>
    {
        Task<User_En> GetUserByEmail(string email);
    }
}
