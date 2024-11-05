using BookStore.Data;
using CSharpFileManager.Models;
using System.IO;
using System.Linq;
using FileManagerServer;

namespace CSharpFileManager.Repository
{
    public static class FileRepository
    {
        private static readonly ApplicationContext _context = Program.DbContext();

        private static async Task AddFileMetadataAsync(FileMetadata fileMetadata)
        {
            Console.WriteLine("Added new file metadata");
            await _context.Files.AddAsync(fileMetadata);
            await _context.SaveChangesAsync();
        }

        public static void UpdateFileMetadata(FileMetadata fileMetadata)
        {
            var existingFile = _context.Files.SingleOrDefault(f => f.FilePath == fileMetadata.FilePath);
            if (existingFile != null)
            {
                existingFile.Size = fileMetadata.Size;
                existingFile.LastModifiedTime = fileMetadata.LastModifiedTime;
                _context.SaveChanges();
            }
        }

        public static void RemoveFileMetadata(string filePath)
        {
            var fileMetadata = _context.Files.SingleOrDefault(f => f.FilePath == filePath);
            if (fileMetadata != null)
            {
                _context.Files.Remove(fileMetadata);
                _context.SaveChanges();
            }
        }
        
        public static (FileMetadata fileMetadata, string owner) FindFileAndOwner(string fileName)
        {
            var fileMetadata = _context.Files.SingleOrDefault(f => f.FileName == fileName);
            return (fileMetadata != null 
                ? (fileMetadata, fileMetadata.Owner) 
                : (null, null))!;
        }

        public static async Task UpdateFileIndex(IEnumerable<FileMetadata> fileMetadataList)
        {
            foreach (var fileMetadata in fileMetadataList)
            {
                var existingFile = _context.Files.SingleOrDefault(f => f.FilePath == fileMetadata.FilePath);
                if (existingFile == null)
                {
                    await AddFileMetadataAsync(fileMetadata);
                }
                else if (existingFile.LastModifiedTime != fileMetadata.LastModifiedTime)
                {
                    existingFile.Size = fileMetadata.Size;
                    existingFile.LastModifiedTime = fileMetadata.LastModifiedTime;
                }
                // else if () TODO: сделай что бы файлы из базы тоже удалялись
                // {
                //     
                // }
            }

            // если записсей не существует - удалить
            var dbFiles = _context.Files.ToList();
            foreach (var dbFile in dbFiles)
            {
                if (!fileMetadataList.Any(f => f.FilePath == dbFile.FilePath))
                {
                    _context.Files.Remove(dbFile);
                }
            }
            _context.SaveChanges();
        }
    }
}
