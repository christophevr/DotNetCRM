using DotNetCRM.Core.Models;

namespace DotNetCRM.Core.Ports.Driven;

public interface IEmailFetcher
{
    Task<Email[]> GetUnreadEmailsAsync(MailboxId mailboxId, CancellationToken ct);
}

public interface IEmailRepository
{
    Task<bool> HasEmail(MailboxId mailboxId, CancellationToken ct);
}