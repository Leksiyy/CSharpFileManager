using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CSharpFileManager.Models;
using CSharpFileManager.Repository;

namespace FileManagerServer.Server;

public class Server
{
    private const int Port = 5000;
    private TcpListener _listener;

    public Server()
    {
        _listener = new TcpListener(IPAddress.Any, Port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Server started on port {Port}.");

        while (true)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // TODO: добавить логику обработки сообщений
                    string response = ProcessMessage(message);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        Console.WriteLine("Client disconnected.");
    }

    private string ProcessMessage1(string message)
    {
        //TODO: добавить логику обработки входящих сообщений
        //Например, проверка логина, получение файлов и т.д.
        return $"{message}"; // просто ответ
    }
    
    private string ProcessAuth(string message)
    {
        var authRequest = JsonSerializer.Deserialize<AuthRequest>(message);
    
        if (AuthRepository.IsLogin(authRequest))
        {
            return "Login successful!";
        }
        else if (AuthRepository.IsRegistration(authRequest))
        {
            return "Registration successful!";
        }
    
        return "Invalid request.";
    }
    
    public string ProcessFileIndexUpdate(string json)
    {
        var fileMetadataList = JsonSerializer.Deserialize<List<FileMetadata>>(json);
        if (fileMetadataList != null)
        {
            FileRepository.UpdateFileIndex(fileMetadataList);
            return "File index updated successfully.";
        }

        return "Failed to update file index.";
    }
    
    private string ProcessMessage(string message)
    {
        string[] parts = message.Split('|');
    
        switch (parts[0])
        {
            case "SYNC":
                string directoryPath = parts[1];
                return ProcessFileIndexUpdate(directoryPath);
            case "AUTH":
                string authRequest = parts[1];
                return ProcessAuth(authRequest);
        
            default:
                return "Unknown command";
        }
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
