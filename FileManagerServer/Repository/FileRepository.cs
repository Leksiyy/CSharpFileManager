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

        public static void AddFileMetadata(FileMetadata fileMetadata)
        {
            _context.Files.Add(fileMetadata);
            _context.SaveChanges();
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

        public static void UpdateFileIndex(IEnumerable<FileMetadata> fileMetadataList)
        {
            foreach (var fileMetadata in fileMetadataList)
            {
                var existingFile = _context.Files.SingleOrDefault(f => f.FilePath == fileMetadata.FilePath);
                if (existingFile == null)
                {
                    _context.Files.Add(fileMetadata);
                }
                else if (existingFile.LastModifiedTime != fileMetadata.LastModifiedTime)
                {
                    existingFile.Size = fileMetadata.Size;
                    existingFile.LastModifiedTime = fileMetadata.LastModifiedTime;
                }
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
