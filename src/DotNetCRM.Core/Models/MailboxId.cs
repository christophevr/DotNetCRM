namespace DotNetCRM.Core.Models;

public readonly struct MailboxId : IEquatable<MailboxId>
{
    private readonly Guid _value;

    private MailboxId(Guid value)
    {
        _value = value;
    }

    public static MailboxId New()
    {
        return new MailboxId(Guid.NewGuid());
    }

    public bool Equals(MailboxId other)
    {
        return _value.Equals(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is MailboxId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static readonly IEqualityComparer<MailboxId> EqualityComparer = EqualityComparer<MailboxId>.Default;
}