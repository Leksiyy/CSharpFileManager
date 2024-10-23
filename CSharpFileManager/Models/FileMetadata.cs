namespace CSharpFileManager.Models;

public class FileMetadata
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string Extension { get; set; }
    public string FilePath { get; set; }
    public long Size { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public DateTime CreationTime { get; set; }
    public string Owner { get; set; }
}

//CREATE TABLE FileMetadata (
//     Id INT PRIMARY KEY IDENTITY,
//     FileName NVARCHAR(255),
//     Extension NVARCHAR(10),
//     Size BIGINT,
//     FilePath NVARCHAR(1000),
//     CreationTime DATETIME,
//     LastModifiedTime DATETIME
// );
// 