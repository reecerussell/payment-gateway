using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Payments.IntegrationTests;

[SetUpFixture]
public class SetupFixture
{
    private INetwork? _network;
    private IContainer? _apiContainer;
    private IContainer? _bankSimulatorContainer;

    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        await CreateNetworkAsync();
        await StartAndCreateBankSimulatorAsync();
        await StartAndCreateApiAsync();
    }
    
    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_apiContainer != null)
        {
            await _apiContainer.StopAsync();
            await _apiContainer.DisposeAsync();
        }

        if (_bankSimulatorContainer != null)
        {
            await _bankSimulatorContainer.StopAsync();
            await _bankSimulatorContainer.DisposeAsync();
        }

        if (_network != null)
        {
            await _network.DeleteAsync();
            await _network.DisposeAsync();
        }
    }

    private async Task CreateNetworkAsync()
    {
        _network = new NetworkBuilder()
            .WithName("payments-gateway")
            .WithDriver(NetworkDriver.Bridge)
            .Build();

        await _network.CreateAsync();
    }

    private async Task StartAndCreateApiAsync()
    {
        var rootDirectory = Path.Join(Environment.CurrentDirectory, "../../../..");
        var image = new ImageFromDockerfileBuilder()
            .WithCleanUp(true)
            .WithDockerfileDirectory(rootDirectory)
            .WithDockerfile("Payments/Dockerfile")
            .Build();

        await image.CreateAsync();
        
        _apiContainer = new ContainerBuilder()
            .WithImage(image)
            .WithEnvironment(new Dictionary<string, string>
            {
                ["BANK_ADDRESS"] = "http://bank-simulator:8080"
            })
            .WithNetwork(_network.Name)
            .WithCleanUp(true)
            .WithPortBinding(8080, 8080)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(8080)))
            .Build();

        await _apiContainer.StartAsync();
    }
    
    private async Task StartAndCreateBankSimulatorAsync()
    {
        var hostVolume = Path.Join(Environment.CurrentDirectory, "../../../../imposters");
        var containerVolume = "/imposters";

        _bankSimulatorContainer = new ContainerBuilder()
            .WithImage("bbyars/mountebank:2.8.1")
            .WithCommand("--configfile", "/imposters/bank_simulator.ejs", "--allowInjection")
            .WithName("bank-simulator")
            .WithNetwork(_network.Name)
            .WithNetworkAliases("bank-simulator")
            .WithBindMount(hostVolume, containerVolume, AccessMode.ReadOnly)
            .WithCleanUp(true)
            .Build();

        await _bankSimulatorContainer.StartAsync();
    }
}