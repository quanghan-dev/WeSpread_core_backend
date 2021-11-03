using BLL.Dto.User;

namespace BLL.Service
{
    public interface IPersistentLoginService
    {
        void StoreUserToken(string token, string userID);

        string GetUserIDByToken(string token);
    }
}
