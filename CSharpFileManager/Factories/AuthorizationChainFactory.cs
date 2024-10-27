using CSharpFileManager.Autorization;

namespace CSharpFileManager.Factories;

public static class AuthorizationChainFactory
{
    public static AuthorizationHandler CreateAuthorizationChain()
    {
        var adminHandler = new AdminAuthorizationHandler();
        var userHandler = new UserAuthorizationHandler();
        var guestHandler = new GuestAuthorizationHandler();

        // Admin -> User -> Guest
        adminHandler.SetNext(userHandler).SetNext(guestHandler);

        return adminHandler;
    }
}
