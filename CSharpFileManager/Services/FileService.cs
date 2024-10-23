using BookStore.Data;
using CSharpFileManager.Models;

namespace CSharpFileManager.Services;

public class FileService
{
    public void IndexFiles(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var filePath in files)
        {
            var fileInfo = new FileInfo(filePath);
            
            string FileName = fileInfo.Name;
            string Extension = fileInfo.Extension;
            string FilePath = filePath;
            long FileSize = fileInfo.Length;
            DateTime CreationTime = fileInfo.CreationTime;
            DateTime LastModifiedTime = fileInfo.LastWriteTime;
            //TODO: add owner 
            
            SaveFileMetadataToDatabase(FileName, Extension, FilePath, FileSize, CreationTime, LastModifiedTime);
        }
    }

    public void SaveFileMetadataToDatabase(string FileName, string Extension, string FilePath, long FileSize, DateTime CreationTime,
        DateTime LastModifiedTime)
    {
        using (ApplicationContext context = Program.DbContext())
        {
            var fileMetadata = new FileMetadata
            {
                FileName = FileName,
                Extension = Extension,
                Size = FileSize,
                FilePath = FilePath,
                CreationTime = CreationTime,
                LastModifiedTime = LastModifiedTime
            };

            context.Files.Add(fileMetadata);
            context.SaveChanges();
        }
    }

    public void UpdateFileIndex(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        using (ApplicationContext context = Program.DbContext())
        {
            foreach (var filePath in files)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                    
                var existingFileMetadata = context.Files.SingleOrDefault(f => f.FilePath == filePath);

                if (existingFileMetadata != null)
                {
                    SaveFileMetadataToDatabase(fileInfo.Name, fileInfo.Extension, filePath, filePath.Length,
                        fileInfo.CreationTime, fileInfo.LastWriteTime);
                } else if (existingFileMetadata.LastModifiedTime != fileInfo.LastWriteTime)
                {
                    existingFileMetadata.Size = fileInfo.Length;
                    existingFileMetadata.LastModifiedTime = fileInfo.LastWriteTime;
                    context.SaveChanges();
                }
                
                List<FileMetadata> fileMetadataList = context.Files.ToList();

                foreach (FileMetadata metadata in fileMetadataList)
                {
                    if (!File.Exists(metadata.FilePath))
                    {
                        context.Files.Remove(metadata);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}