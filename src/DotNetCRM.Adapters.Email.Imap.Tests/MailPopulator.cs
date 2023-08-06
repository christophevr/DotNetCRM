using System.Diagnostics;
using Bogus;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;

namespace DotNetCRM.Adapters.Email.Imap.Tests;

public class MailPopulator : IAsyncLifetime
{
    private MailServerContainer _mailServer = null!;

    public async Task InitializeAsync()
    {
        _mailServer = await MailServerContainer.StartNewAsync();
    }

    public async Task DisposeAsync()
    {
        await _mailServer.DisposeAsync();
        Debugger.Break();
    }

    [Fact]
    public async Task PopulateMailboxWithFakeData()
    {
        using var smtpClient = new SmtpClient(new ProtocolLogger (Console.OpenStandardOutput ()));
        await smtpClient.ConnectAsync(_mailServer.Hostname, _mailServer.SmtpPort);
        await smtpClient.AuthenticateAsync("user", "password");

        var recipient = new MailboxAddress("Recipient McRecipientface", "recipient@foo.com");
        var senderFaker = new Faker<MailboxAddress>()
            .CustomInstantiator(f => new MailboxAddress(f.Person.FullName, f.Person.Email));

        var messageFaker = new Faker<MimeMessage>()
            .RuleFor(x => x.MessageId, MimeUtils.GenerateMessageId)
            .RuleFor(x => x.Subject, f => f.Lorem.Sentence())
            .RuleFor(x => x.Body, f => new TextPart {Text = f.Lorem.Paragraphs()});
        
        for (var i = 0; i < 100; i++)
        {
            var message = messageFaker.Generate();
            message.From.Add(senderFaker.Generate());
            message.To.Add(recipient);
            
            await smtpClient.SendAsync(
                FormatOptions.Default,
                message);
        }
    }
}

