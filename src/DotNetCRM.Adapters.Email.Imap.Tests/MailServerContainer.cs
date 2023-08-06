using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace DotNetCRM.Adapters.Email.Imap.Tests;

public class MailServerContainer : IAsyncDisposable
{
    private const int InternalImapPort = 3143;
    private const int InternalSmtpPort = 3025;
    
    private readonly IContainer _mailServer;
    private readonly IContainer _mailWebClient;
    private readonly string _networkAlias;

    public string Hostname => _mailServer.Hostname;
    public int ImapPort => _mailServer.GetMappedPublicPort(InternalImapPort);
    public int SmtpPort => _mailServer.GetMappedPublicPort(InternalSmtpPort);

    private MailServerContainer(IContainer mailServer, IContainer mailWebClient, string networkAlias)
    {
        _mailServer = mailServer;
        _mailWebClient = mailWebClient;
        _networkAlias = networkAlias;
    }

    public static async Task<MailServerContainer> StartNewAsync(
        string hostname = "mailserver",
        CancellationToken ct = default)
    {
        var network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();
            
        var mailServer = new ContainerBuilder()
            .WithImage("greenmail/standalone:latest")
            .WithPortBinding(13143, InternalImapPort)
            .WithPortBinding(13025, InternalSmtpPort)
            .WithNetwork(network)
            .WithNetworkAliases(hostname)
            .Build();
            
        var mailWebClient = new ContainerBuilder()
            .WithImage("roundcube/roundcubemail")
            .WithPortBinding(8000, 80)
            .WithEnvironment(new Dictionary<string, string>
            {
                {"ROUNDCUBEMAIL_DEFAULT_HOST", hostname},
                {"ROUNDCUBEMAIL_SMTP_SERVER", hostname},
                {"ROUNDCUBEMAIL_DEFAULT_PORT", "3143"},
            })
            .DependsOn(mailServer)
            .WithNetwork(network)
            .Build();

        await Task.WhenAll(
            mailServer.StartAsync(ct),
            mailWebClient.StartAsync(ct),
            Task.Delay(TimeSpan.FromSeconds(5)));

        return new MailServerContainer(
            mailServer,
            mailWebClient,
            hostname);
    }

    public async ValueTask DisposeAsync()
    {
        await _mailServer.StopAsync();
        await _mailWebClient.StopAsync();
    }
}