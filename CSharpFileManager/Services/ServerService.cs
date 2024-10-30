using System.Net.Sockets;
using System.Text;

namespace CSharpFileManager.Services;

public class ServerService : IDisposable
{
    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 5000;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;

    private async Task ConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ServerIp, ServerPort);
            _networkStream = _tcpClient.GetStream();
            Console.WriteLine("Connected to the server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
    }

    public async Task SendAsync(string message) // а это уже для метадаты
    {
        if (_networkStream == null!)
        {
            await ConnectAsync();
        }

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
        if (_networkStream == null!)
        {
            await ConnectAsync();
        }
        
        _networkStream!.WriteAsync(message, 0, message.Length);
    }
    
    public async Task<string?> ReceiveAsync()
    {
        if (_networkStream == null!)
        {
            await ConnectAsync();
        }

        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
            Disconnect();
            return null;
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
            return _tcpClient?.Connected == true && (_networkStream?.CanRead ?? false);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}

