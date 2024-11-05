using System.Text.Json;
using CSharpFileManager.Models;
using CSharpFileManager.Utilities;

namespace CSharpFileManager.Services;

public class MenuService
{
    private bool _isAuthenticated = false;
    
    public async Task RunAsync()
    {
        await AuthenticateAsync();

        Program.HomePath = ConfigurationService.SetConfig();
        
        while (_isAuthenticated)
        {
            Console.Clear();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Sync files");
            Console.WriteLine("2. Download file from another client");
            Console.WriteLine("2. Upload File to other client");
            Console.WriteLine("3. View available files");
            Console.WriteLine("4. Exit");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await SyncFilesAsync();
                    break;
                case "2":
                    await DownloadFileAsync();
                    break;
                case "3":
                    await UploadFileAsync();
                    break;
                case "4":
                    await ViewAvailableFilesAsync();
                    break;
                case "5":
                    _isAuthenticated = false;
                    Console.WriteLine("Exiting...");
                    break;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }

            if (_isAuthenticated)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private async Task AuthenticateAsync()
    {
        while (!_isAuthenticated)
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            var isAuthSuccessful = await Program._serverService.AuthenticateAsync(username, password);
            Console.WriteLine($"Authenticated: {isAuthSuccessful}");
            if (isAuthSuccessful)
            {
                _isAuthenticated = true;
                AuthService.Username = username;
                Console.WriteLine("Authentication successful!");
            }
            else
            {
                Console.WriteLine("Authentication failed. Try again.");
            }
        }
    }

    private async Task SyncFilesAsync()
    {
        Console.WriteLine("Starting file synchronization...");
        await Program._fileService.SyncFilesAsync(Program.HomePath.HomeDirectory); // Путь к папке
        Console.WriteLine("File synchronization completed.");
    }

    private async Task DownloadFileAsync()
    {
        Console.Write("Enter the name of the file to download: ");
        string fileName = Console.ReadLine();
        
        bool downloadSuccessful = await Program._serverService.RequestFileAsync(fileName);
        
        if (downloadSuccessful)
            Console.WriteLine("File downloaded successfully.");
        else
            Console.WriteLine("Failed to download file or file not found.");
    }
    
    private async Task ViewAvailableFilesAsync()
    {
        Console.WriteLine("Retrieving available files...");
        string fileListJson = await Program._serverService.GetAvailableFilesAsync()!;

        if (!string.IsNullOrEmpty(fileListJson))
        {
            var files = JsonSerializer.Deserialize<List<FileMetadata>>(fileListJson);

            Console.WriteLine("Available files:");
            foreach (var file in files)
            {
                Console.WriteLine($"{file.FileName} - Size: {file.Size} bytes, Last modified: {file.LastModifiedTime}");
            }
        }
        else
        {
            Console.WriteLine("No files available or unable to retrieve file list.");
        }
    }

    private async Task UploadFileAsync()
    {
        Console.Write("Starting upload requested file...");
        await Program._serverService.ListenForMessagesAsync();
    }
}
