using System.Text;
using DotNet.Testcontainers.Configurations;

namespace Payments.IntegrationTests;

public class OutputConsumer : IOutputConsumer
{
    private readonly string _name;
    private Thread _consumer;
    
    public bool Enabled => true;
    public Stream Stdout { get; }
    public Stream Stderr { get; }

    public OutputConsumer(string name)
    {
        _name = name;
        Stdout = new MemoryStream();
        Stderr = new MemoryStream();

        _consumer = new Thread( () =>
        {
            Task.Run(() => ListenToStreamAsync(Stdout));
        });
        _consumer.Start();
    }

    private async Task ListenToStreamAsync(Stream stream)
    {
        stream.Position = 0;
        
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        while (await reader.ReadLineAsync() is { } line)
        {
            await TestContext.Error.WriteLineAsync($"[{_name}]: {line}");
        }
    }

    public void Dispose()
    {
        _consumer.Join();
    }
}