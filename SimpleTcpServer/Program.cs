using System.Net;
using System.Net.Sockets;
using System.Text;

var listener = new TcpListener(IPAddress.Loopback, 5014);
listener.Start(backlog: 0);
Console.WriteLine("Server started on port 5014");

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    _ = HandleClientAsync(client); // Fire and forget Task runs on a thread pool thread
}

static async Task HandleClientAsync(TcpClient client)
{
    await using var stream = client.GetStream();
    var buffer = new byte[1024];
    Console.WriteLine("Client connected");

    try
    {
        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead == 0) // FIN packet received
            {
                Console.WriteLine("Client sent FIN packet (connection closing).");
                break; // Exit the loop, client is closing the connection
            }

            // Log and process the received data
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {message}");
            
            // Echo back to client
            await stream.WriteAsync(Encoding.UTF8.GetBytes($"The server received this message from you: {message}"));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        Console.WriteLine("Client disconnected");
    }
}