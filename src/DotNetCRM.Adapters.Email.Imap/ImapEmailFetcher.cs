using System.Text;
using DotNetCRM.Core.Models;
using DotNetCRM.Core.Ports.Driven;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace DotNetCRM.Adapters.Email.Imap;

public class ServiceNotAuthenticatedException : Exception
{
    public ServiceNotAuthenticatedException(string message) : base(message)
    {
    }
}

public class ImapEmailFetcher : IEmailFetcher
{
    private readonly IMailboxSettingsProvider _settingsProvider;
    private readonly ILogger<ImapEmailFetcher> _logger;

    public ImapEmailFetcher(
        IMailboxSettingsProvider settingsProvider,
        ILogger<ImapEmailFetcher> logger)
    {
        _settingsProvider = settingsProvider;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mailboxId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="ServiceNotAuthenticatedException"></exception>
    public async Task<Core.Models.Email[]> GetUnreadEmailsAsync(
        MailboxId mailboxId,
        CancellationToken ct)
    {
        var config = await _settingsProvider.FindSettingsForMailboxAsync(mailboxId, ct);
        if (config == null)
        {
            throw new KeyNotFoundException("Mailbox not found");
        }

        var imapSettings = config.ImapSettings;
        using var client = new ImapClient();

        try
        {
            await client.ConnectAsync(imapSettings.Host, imapSettings.Port, SecureSocketOptions.Auto, ct);
            await client.AuthenticateAsync(imapSettings.UserName, imapSettings.Password, ct);

            var inbox = client.Inbox;

            await inbox.OpenAsync(FolderAccess.ReadOnly, ct);
            var unreadMessageIds = await inbox.SearchAsync(SearchQuery.NotSeen, ct);
            var emails = new List<Core.Models.Email>();

            foreach (var messageId in unreadMessageIds)
            {
                var message = await inbox.GetMessageAsync(messageId, ct);

                emails.Add(new Core.Models.Email(message.MessageId));
            }

            return emails.ToArray();
        }
        catch (MailKit.ServiceNotAuthenticatedException e)
        {
            throw new ServiceNotAuthenticatedException(e.Message);
        }
    }
}