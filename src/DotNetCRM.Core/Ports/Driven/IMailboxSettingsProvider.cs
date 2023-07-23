using DotNetCRM.Core.Models;

namespace DotNetCRM.Core.Ports.Driven;

public interface IMailboxSettingsProvider
{
    Task<MailboxSettings?> FindSettingsForMailboxAsync(MailboxId mailboxId, CancellationToken ct);
}