using System.Text.Json;
using CSharpFileManager.Models;

namespace CSharpFileManager.Services;

public class FileService
{
    private readonly Dictionary<string, DateTime> _localFileIndex = new Dictionary<string, DateTime>(); // хранит последнюю дату изменения для отслеживания

    public async Task SyncFilesAsync(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        var updatedFiles = new List<FileMetadata>();

        foreach (var filePath in files)
        {
            var fileInfo = new FileInfo(filePath);
            if (!_localFileIndex.ContainsKey(filePath) || _localFileIndex[filePath] != fileInfo.LastWriteTime)
            {
                // только если он новый или изменился
                updatedFiles.Add(new FileMetadata
                {
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    FilePath = filePath,
                    Size = fileInfo.Length,
                    CreationTime = fileInfo.CreationTime,
                    LastModifiedTime = fileInfo.LastWriteTime,
                    Owner = AuthService.Username
                });

                // обновляю индекс
                _localFileIndex[filePath] = fileInfo.LastWriteTime;
            }
        }
        //TODO: сделвй уже что бы учитывалось и удаление файлов, у тебя же уже написан метод для этого
        if (updatedFiles.Any())
        {
            string json = JsonSerializer.Serialize(updatedFiles);
            await Program._serverService.SendAsync("SYNC|" + json);
            Console.WriteLine("Updated file metadata sent to the server.");
        }
        else
        {
            Console.WriteLine("No updates found for file metadata.");
        }
    }
    
    // не используеться птому что придуман другой метод в сервер сервис, а это пока что запасной
    //TODO: может удалишь его уже?
    // private async Task HandleFileSendRequest(string message) 
    // {
    //     string[] parts = message.Split('|');
    //     string fileName = parts[1];
    //     string requestingClientId = parts[2];
    //
    //     // читаю и отправляю файл
    //     if (File.Exists(fileName))
    //     {
    //         using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
    //         byte[] buffer = new byte[1024];
    //         int bytesRead;
    //     
    //         while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
    //         {
    //             await Program._serverService.SendBytesAsync(buffer);
    //         }
    //     
    //         Console.WriteLine($"File '{fileName}' sent to {requestingClientId}.");
    //     }
    //     else
    //     {
    //         Console.WriteLine($"File '{fileName}' not found.");
    //     }
    // }

}
