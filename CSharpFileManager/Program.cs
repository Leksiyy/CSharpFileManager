using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using CSharpFileManager.Autorization;
using CSharpFileManager.Models;
using CSharpFileManager.Services;

namespace CSharpFileManager;

class Program
{
    public static ServerService _serverService = new ServerService(); // сервак с интерфейсом для всей программы
    public static Config HomePath = ConfigurationService.SetConfig(); // конфигурация домашней директории
    
    static async Task Main(string[] args)
    {
        if (_serverService.IsConnected())
        {
            await AuthService.LoginOrRegisterAsync();
            FileService fileService = new FileService();
            await fileService.SyncFilesAsync(HomePath.HomeDirectory);
        }
    }
}