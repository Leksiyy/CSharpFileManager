using CSharpFileManager.Models;

namespace CSharpFileManager.Autorization;

public class GuestAuthorizationHandler : AuthorizationHandler
{
    public override void Handle(User user, Action action)
    {
        if (user.Role == UserRoleEnum.Guest)
        {
            Console.WriteLine("Guest access granted.");
            action();  // TODO: AuthorizationHandler action();
        }
        else
        {
            _nextHandler?.Handle(user, action);
        }
    }
}