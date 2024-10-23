using CSharpFileManager.Models;

namespace CSharpFileManager.Autorization;

public class AdminAuthorizationHandler : AuthorizationHandler
{
    public override void Handle(User user, Action action)
    {
        if (user.Role == UserRoleEnum.Admin)
        {
            Console.WriteLine("Admin access granted.");
            action();  // TODO: AuthorizationHandler action();
        }
        else
        {
            _nextHandler?.Handle(user, action);
        }
    }
}