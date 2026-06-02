using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Security;

namespace SchoolManagementSystem.BLL
{
    public class AuthService
    {

        private readonly UserRepository repo = new UserRepository();

        public Models.User Login(string username, string password)
        {
            var user = repo.GetUserByUsername(username);

            if (user == null)
                return null;

            bool isValid = PasswordHelper.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt
            );

            return isValid ? user : null;
        }

        public void ChangePassword(int userId, string newPassword)
        {
            PasswordHelper.CreatePasswordHash(newPassword, out var hash, out var salt);
            repo.UpdatePassword(userId, hash, salt);
        }
    }
}