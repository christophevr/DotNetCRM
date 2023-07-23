using System.Security;

namespace DotNetCRM.Core.Models;

public class MailboxSettings
{
    public MailboxSettings(
        ImapSettings imapSettings,
        LabelSettings labelSettings)
    {
        ImapSettings = imapSettings;
        LabelSettings = labelSettings;
    }

    public ImapSettings ImapSettings { get; }
    public LabelSettings LabelSettings { get; }
}

public class LabelSettings
{
    public class UponRead
    {
        public UponRead(
            bool shouldApplyLabel,
            string labelToApply)
        {
            ShouldApplyLabel = shouldApplyLabel;
            LabelToApply = labelToApply;
        }

        public bool ShouldApplyLabel { get; }
        public string LabelToApply { get; }
    }

}


public class ImapSettings
{
    public ImapSettings(
        string host,
        int port,
        bool enableTls, 
        string userName,
        string password)
    {
        Host = host;
        Port = port;
        EnableTls = enableTls;
        UserName = userName;
        Password = password;
    }

    public string Host { get; }
    public int Port { get; }
    public bool EnableTls { get; }
    public string UserName { get; }
    public string Password { get; }
}