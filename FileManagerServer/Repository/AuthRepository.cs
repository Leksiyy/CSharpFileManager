using BookStore.Data;
using CSharpFileManager.Models;
using CSharpFileManager.Utilities;
using FileManagerServer;

namespace CSharpFileManager.Repository;

public class AuthRepository
{
    public static bool IsLogin(AuthRequest authRequest)
    {
        using (ApplicationContext context = Program.DbContext())
        {
            var user = context.Users.SingleOrDefault(u => u.Name == authRequest.Username);
                
            if (user != null)
            {
                return HashingUtils.VerifyPassword(authRequest.Password, user.Password);
            }
        }
        return false;
    }

    public static bool IsRegistration(AuthRequest authRequest)
    {
        using (ApplicationContext context = Program.DbContext())
        {
            if (context.Users.Any(u => u.Name == authRequest.Username))
            {
                Console.WriteLine("User already exists.");
                return false;
            }

            var newUser = new User
            {
                Name = authRequest.Username,
                Password = HashingUtils.HashPassword(authRequest.Password),
                Role = UserRoleEnum.User
            };

            context.Users.Add(newUser);
            context.SaveChanges();

            Console.WriteLine("User registered successfully.");
            return true;
        }
    }
}
