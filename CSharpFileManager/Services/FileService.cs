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

                // Обновляю индекс
                _localFileIndex[filePath] = fileInfo.LastWriteTime;
            }
        }

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
}
