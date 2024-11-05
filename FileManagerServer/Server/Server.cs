using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using CSharpFileManager.Models;
using CSharpFileManager.Repository;

namespace FileManagerServer.Server;

public class Server
{
    private const int Port = 5050;
    private readonly TcpListener _listener = new(IPAddress.Any, Port);

    // потокобезопасное хранение подключённых клиентов
    public static ConcurrentDictionary<string, TcpClient> OnlineClients { get; private set; } = new();

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
        string clientId = null;

        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                // сначала отдельно читаю аутентификацию
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string authMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received auth message: " + authMessage);
                string authResponse = RequestProcessor.ProcessAuth(authMessage, out clientId, client);

                OnlineClients.TryAdd(clientId, client);
                
                // потом отправляю ответ на аутентификацию
                byte[] responseData = Encoding.UTF8.GetBytes(authResponse);
                Console.WriteLine("Sending auth response: " + authResponse);
                await stream.WriteAsync(responseData, 0, responseData.Length);

                while (true)
                {
                    // ну и потом можно обмениваться данными
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {message}");

                        string response = ProcessMessage(message, clientId);
                        responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                // удалил клиента из OnlineClients при отключении
                if (clientId != null)
                {
                    OnlineClients.TryRemove(clientId, out _);
                    Console.WriteLine($"Client {clientId} disconnected.");
                }
            }
        }
    }
    
    private string ProcessMessage(string message, string clientId)
    {
        string[] parts = message.Split('|');
        return Convert.ToString(parts[0] switch
        {
            "SYNC" => RequestProcessor.ProcessFileIndexUpdate(parts[1]),
            "REQUEST_FILE" => RequestProcessor.ProcessFileRequest(parts[1], clientId),
            _ => "Unknown command"
        })!;
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
