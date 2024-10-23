using CSharpFileManager.Models;

namespace CSharpFileManager.Autorization;

public class UserAuthorizationHandler : AuthorizationHandler
{
    public override void Handle(User user, Action action)
    {
        if (user.Role == UserRoleEnum.User)
        {
            Console.WriteLine("User access granted.");
            action();  // TODO: AuthorizationHandler action();
        }
        else
        {
            _nextHandler?.Handle(user, action);
        }
    }
}