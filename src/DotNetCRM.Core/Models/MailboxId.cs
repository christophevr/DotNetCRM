namespace DotNetCRM.Core.Models;

public readonly record struct MailboxId(Guid Value)
{
    public static MailboxId New()
    {
        return new MailboxId(Guid.NewGuid());
    }
}