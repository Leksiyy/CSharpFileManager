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
    private const int Port = 5000;
    private TcpListener _listener;

    // хранение подключённых клиентов
    public static ConcurrentDictionary<string, TcpClient> OnlineClients { get; private set; } = new();

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
                string authResponse = ProcessAuth(authMessage, out clientId, client);

                OnlineClients.TryAdd(clientId, client);
                
                // потом отправляю ответ на аутентификацию
                byte[] responseData = Encoding.UTF8.GetBytes(authResponse);
                await stream.WriteAsync(responseData, 0, responseData.Length);

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

    private string ProcessAuth(string message, out string clientId, TcpClient client)
    {
        var authRequest = JsonSerializer.Deserialize<AuthRequest>(message);
        clientId = authRequest?.Username;

        if (clientId != null)
        {
            if (AuthRepository.IsLogin(authRequest))
            {
                // добавил клиента в коллекцию подключённых клиентов
                OnlineClients.TryAdd(clientId, client );
                return "Login successful!";
            }
            else if (AuthRepository.IsRegistration(authRequest))
            {
                return "Registration successful!";
            }
        }

        return "Invalid request.";
    }

    private string ProcessFileIndexUpdate(string json)
    {
        var fileMetadataList = JsonSerializer.Deserialize<List<FileMetadata>>(json);
        if (fileMetadataList != null)
        {
            FileRepository.UpdateFileIndex(fileMetadataList);
            return "File index updated successfully.";
        }

        return "Failed to update file index.";
    }

    private async Task<string> ProcessFileRequest(string requestedFileName, string requestingClientId)
    {
        var (fileMetadata, owner) = FileRepository.FindFileAndOwner(requestedFileName);

        if (fileMetadata != null)
        {
            if (OnlineClients.ContainsKey(owner))
            {
                // Находим TCP клиент владельца файла
                var ownerClient = OnlineClients.FirstOrDefault(c => c.Key == owner).Value;

                if (ownerClient != null)
                {
                    // Создаем запрос на отправку файла
                    string requestMessage = $"SEND_FILE|{requestedFileName}|{requestingClientId}";
                    await SendMessageToClient(ownerClient, requestMessage);
                    return $"File '{requestedFileName}' request sent to {owner}.";
                }
                else
                {
                    return "Owner of the file is not online.";
                }
            }
            else
            {
                return "Owner of the file is not online.";
            }
        }
        else
        {
            return "File not found.";
        }
    }

    private async Task SendMessageToClient(TcpClient client, string message)
    {
        if (client != null && client.Connected)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
    }

    //просто перенаправляю битовый поток с клиента файла на запрашивающий клиент
    private async Task HandleClientFileRequest(TcpClient ownerClient, TcpClient requestingClient)
    {
        using NetworkStream ownerStream = ownerClient.GetStream();
        using NetworkStream requestStream = requestingClient.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;
        
        while ((bytesRead = await ownerStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await requestStream.WriteAsync(buffer, 0, bytesRead);
        }

        Console.WriteLine("File transferred from owner to requester.");
    }


    private string ProcessMessage(string message, string clientId)
    {
        string[] parts = message.Split('|');
        return Convert.ToString(parts[0] switch
        {
            "SYNC" => ProcessFileIndexUpdate(parts[1]),
            "AUTH" => ProcessAuth(parts[1], out _, null),
            "REQUEST_FILE" => ProcessFileRequest(parts[1], clientId),
            _ => "Unknown command"
        });
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
