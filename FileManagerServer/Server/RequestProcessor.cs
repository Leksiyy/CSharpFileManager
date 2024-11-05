using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CSharpFileManager.Models;
using CSharpFileManager.Repository;

namespace FileManagerServer.Server;

public static class RequestProcessor
{
    
    //просто перенаправляю битовый поток с клиента файла на запрашивающий клиент
    public static async Task HandleClientFileRequest(TcpClient ownerClient, TcpClient requestingClient)
    {
        using NetworkStream ownerStream = ownerClient.GetStream();
        using NetworkStream requestStream = requestingClient.GetStream();

        byte[] buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await ownerStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await requestStream.WriteAsync(buffer, 0, bytesRead);
        }

        Console.WriteLine("File transferred from owner to requester.");
    }
    
    public static async Task<string> ProcessFileRequest(string requestedFileName, string requestingClientId)
    {
        var (fileMetadata, owner) = FileRepository.FindFileAndOwner(requestedFileName);

        if (Server.OnlineClients.ContainsKey(owner))
        {
            // нахожу TcpClient владельца
            var ownerClient = Server.OnlineClients[owner];
            var requestingClient = Server.OnlineClients[requestingClientId];

            // отправляю запрос владельцу файла на его отправку
            string requestMessage = $"SEND_FILE|{requestedFileName}|{requestingClientId}";
            await SendMessageToClient(ownerClient, requestMessage);

            // после отправки запроса владельцу начинаею перенаправлять поток от владельца к запрашивающему клиенту
            await HandleClientFileRequest(ownerClient, requestingClient);
            return $"File '{requestedFileName}' transferred to {requestingClientId}.";
        }
        else
        {
            return "Owner of the file is not online.";
        }
    }
    
    public static async Task SendMessageToClient(TcpClient client, string message)
    {
        if (client != null && client.Connected)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
    }
    
    public static string ProcessAuth(string message, out string clientId, TcpClient client)
    {
        string[] splitMessage = message.Split('|');
        var authRequest = JsonSerializer.Deserialize<AuthRequest>(splitMessage[1]);
        clientId = authRequest.Username;

        if (clientId != null)
        {
            if (AuthRepository.IsLogin(authRequest)) //TODO: не позорься и переделай проверки что бы они проверяли а не просто делали
            {
                // добавил клиента в коллекцию подключённых клиентов
                Server.OnlineClients.TryAdd(clientId, client );
                return "Login successful!";
            }
            else /*серое потому что проверки по сути нет*/ if (AuthRepository.IsRegistration(authRequest))
            {
                return "Registration successful!";
            }
        }

        return "Invalid request.";
    }
    
    public static string ProcessFileIndexUpdate(string json)
    {
        var fileMetadataList = JsonSerializer.Deserialize<List<FileMetadata>>(json);
        if (fileMetadataList != null)
        {
            FileRepository.UpdateFileIndex(fileMetadataList);
            return "File index updated successfully.";
        }

        return "Failed to update file index.";
    }
}