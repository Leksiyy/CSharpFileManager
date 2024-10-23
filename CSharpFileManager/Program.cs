using BookStore.Data;
using CSharpFileManager.Autorization;
using CSharpFileManager.Data;

namespace CSharpFileManager;

class Program
{
    public static ApplicationContext DbContext() => new ApplicationContextFactory().CreateDbContext();

    static void Main(string[] args)
    {

        Console.WriteLine("Hello, World!");
    }
}