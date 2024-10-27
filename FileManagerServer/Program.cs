using BookStore.Data;

namespace FileManagerServer;

class Program
{
    public static ApplicationContext DbContext() => new ApplicationContextFactory().CreateDbContext();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var server = new Server.Server();
        await server.StartAsync();
    }
}