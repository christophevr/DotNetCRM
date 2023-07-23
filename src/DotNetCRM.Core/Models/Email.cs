namespace DotNetCRM.Core.Models;

public class Email
{
    public string UniqueMessageId { get; }

    public Email(string uniqueMessageId)
    {
        UniqueMessageId = uniqueMessageId;
    }
}

