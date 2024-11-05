using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CSharpFileManager.Models;
using CSharpFileManager.Utilities;

namespace CSharpFileManager.Services;

public class ServerService : IDisposable
{
    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 5050;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;

    public async Task ConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ServerIp, ServerPort);
            _networkStream = _tcpClient.GetStream();
            Console.WriteLine(_networkStream.CanRead ? "Connected to the server." : "Could not connect to the server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
    }

    public async Task SendAsync(string message) // а это уже для метадаты
    {

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _networkStream!.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            Disconnect();
        }
    }

    public async Task SendBytesAsync(byte[] message) // для передачи файлов, а не метадаты
    {
        
        _networkStream!.WriteAsync(message, 0, message.Length);
    }
    
    public async Task<string?> ReceiveAsync()
    {
        try
        {
            Console.WriteLine("Start receiving messages response");
            List<byte> receivedData = new List<byte>();
            byte[] buffer = new byte[1024];
            int bytesRead;

            // Чтение данных до тех пор, пока поток возвращает данные
            
            while ((bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                Console.WriteLine($"Bytes read: {bytesRead}");
                receivedData.AddRange(buffer.Take(bytesRead));

                // Прерываем цикл, если данных больше нет
                if (bytesRead < buffer.Length)
                {
                    break;
                }
            }

            if (receivedData.Count == 0)
            {
                Console.WriteLine("No data received or connection closed.");
                return null;
            }

            string response = Encoding.UTF8.GetString(receivedData.ToArray());
            Console.WriteLine($"Response received: {response}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
            Disconnect();
            return null;
        }
    }
    
    public async Task<bool> AuthenticateAsync(string username, string password) // аутентификация
    {
        AuthRequest authRequest = new AuthRequest { Username = username, Password = password };
        string authMessage = $"AUTH|{JsonSerializer.Serialize(authRequest)}";
        Console.WriteLine("LOG:" + authMessage);
        await SendAsync(authMessage);

        string response = await ReceiveAsync();
        Console.WriteLine("LOG:" + response);
        return response == "Login successful!";
    }

    public async Task<string?> GetAvailableFilesAsync() // получить список файлов
    {
        string request = "GET_FILE_LIST|";
        await SendAsync(request);

        return await ReceiveAsync(); // вернёт JSON
    }

    public async Task<bool> RequestFileAsync(string fileName) // запрос на загрузку файла
    {
        string request = $"REQUEST_FILE|{fileName}";
        await SendAsync(request);

        string? response = await ReceiveAsync();
        if (response == "File transfer starting")
        {
            await ReceiveFileAsync(fileName);
            return true;
        }
        else
        {
            Console.WriteLine("File request failed or file not found.");
            return false;
        }
    }
    private async Task ReceiveFileAsync(string fileName) // сохраняю в домашнюю директорию
    {
        using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
            }
        }
        Console.WriteLine($"File '{fileName}' received successfully.");
    }

    public async Task ListenForMessagesAsync()
    {
        try
        {
            while (_tcpClient.Connected)
            {
                string response = await ReceiveAsync();
                if (response is not null)
                {
                    Console.WriteLine($"Message from server: {response}"); 
                    await HandleServerMessage(response);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening for messages: {ex.Message}");
        }
    }

    private async Task HandleServerMessage(string message)
    {
        string[] messageParts = message.Split("|");
        if (messageParts[0] == "SEND_FILE")
        {
            await SendFileAsync(messageParts[1]);
        }
    }
    
    public async Task SendFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[8192]; // Размер буфера для оптимизации передачи данных
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await _networkStream.WriteAsync(buffer, 0, bytesRead);
                }

                Console.WriteLine($"File '{Path.GetFileName(filePath)}' sent successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending file: {ex.Message}");
        }
    }
    

    public void Disconnect()
    {
        _networkStream?.Close();
        _tcpClient?.Close();
        Console.WriteLine("Disconnected from the server.");
    }

    public bool IsConnected()
    {
        try
        {
            return _tcpClient?.Connected == true && _networkStream?.CanRead == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}

