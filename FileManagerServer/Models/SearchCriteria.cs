namespace CSharpFileManager.Models;

public class SearchCriteria
{
    public string FileName { get; set; }
    public string Extension { get; set; }
    public DateTime? CreationDateFrom { get; set; }
    public DateTime? CreationDateTo { get; set; }
    public long? SizeFrom { get; set; }
    public long? SizeTo { get; set; }
}