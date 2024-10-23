using CSharpFileManager.Models;

namespace CSharpFileManager.Autorization;

public abstract class AuthorizationHandler
{
    protected AuthorizationHandler _nextHandler;

    public AuthorizationHandler SetNext(AuthorizationHandler nextHandler)
    {
        _nextHandler = nextHandler;
        return _nextHandler;
    }

    public abstract void Handle(User user, Action action);
}
