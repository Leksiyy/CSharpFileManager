using System.Collections.Concurrent;
using System.Net.Sockets;
using BookStore.Data;
using CSharpFileManager.Models;
using CSharpFileManager.Utilities;

namespace FileManagerServer;

class Program
{
    public static ApplicationContext DbContext() => new ApplicationContextFactory().CreateDbContext();

    static async Task Main(string[] args)
    {
        // using (ApplicationContext db = DbContext())
        // {
        //     await db.Database.EnsureDeletedAsync();
        //     await db.Database.EnsureCreatedAsync();
        //     
        //     var user = new User{ Name = "hello", Password = HashingUtils.HashPassword("world") };
        //     
        //     db.Users.Add(user);
        //     await db.SaveChangesAsync();
        // }
        Console.WriteLine("Hello, World!");
        var server = new Server.Server();
        await server.StartAsync();
    }
}

//TODO: у тебя проблемы с десереализацией, сделай так что бы тип комманды был в json а не просто был рядом