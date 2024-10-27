using System.Text.Json;
using CSharpFileManager.Models;

namespace CSharpFileManager.Services;

public static class AuthService
{
    public static string Username { get; set; }

    private static async Task<string> LoginAsync(string username, string password)
    {
        var authRequest = new AuthRequest { Username = username, Password = password };
        string jsonRequest = "AUTH|" + JsonSerializer.Serialize(authRequest);

        await Program._serverService.SendAsync(jsonRequest);
        return await Program._serverService.ReceiveAsync();
    }

    private static async Task<string> RegisterAsync(string username, string password)
    {
        var authRequest = new AuthRequest { Username = username, Password = password };
        string jsonRequest = "AUTH|" + JsonSerializer.Serialize(authRequest);

        await Program._serverService.SendAsync(jsonRequest);
        return await Program._serverService.ReceiveAsync();
    }
    
    private static async Task LoginAsync()
    {
        Console.Write("Username: ");
        string username = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        string response = await LoginAsync(username, password);
        Console.WriteLine($"Server response: {response}");
    }

    private static async Task RegisterAsync()
    {
        Console.Write("Choose a username: ");
        string username = Console.ReadLine();

        Console.Write("Choose a password: ");
        string password = Console.ReadLine();

        string response = await RegisterAsync(username, password);
        Console.WriteLine($"Server response: {response}");
    }

    public static async Task LoginOrRegisterAsync()
    {
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Register");
        Console.Write("Choose an option: ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            await LoginAsync();
        }
        else if (choice == "2")
        {
            await RegisterAsync();
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }

        Program._serverService.Disconnect();
    }
}
