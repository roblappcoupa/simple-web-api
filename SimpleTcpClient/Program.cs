using System.Net.Sockets;
using System.Text;

const string server = "127.0.0.1"; // Server IP
const int port = 5014; // Server port

using (var client = new TcpClient())
{
    try
    {
        Console.WriteLine($"Connecting to server at {server}:{port}...");
        await client.ConnectAsync(server, port);
        Console.WriteLine("Connected!");

        await using var stream = client.GetStream();
        Console.WriteLine("Enter data to send");
        while (Console.ReadLine() is { } line)
        {
            if (string.Equals("x", line, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            
            var data = Encoding.UTF8.GetBytes(line);
            await stream.WriteAsync(data);
            Console.WriteLine($"Sent: {line}");
            
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {response}");
            
            Console.WriteLine("Enter data to send");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

Console.WriteLine("Client disconnected.");
