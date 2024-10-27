using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using CSharpFileManager.Autorization;
using CSharpFileManager.Models;
using CSharpFileManager.Services;

namespace CSharpFileManager;

class Program
{
    public static ServerService _serverService = new ServerService();
    public static Config HomePath = ConfigurationService.SetConfig();
    static void Main(string[] args)
    {
        AuthService.LoginOrRegisterAsync();
        FileService fileService = new FileService();
        fileService.SyncFilesAsync(HomePath.HomeDirectory);
        
    }
}