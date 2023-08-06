using AutoFixture;
using DotNetCRM.Core.Models;
using DotNetCRM.Core.Ports.Driven;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetCRM.Adapters.Email.Imap.Tests
{
    public class ImapEmailFetcherIntegrationTest : IAsyncLifetime
    {
        private Fixture _fixture = null!;
        private ImapEmailFetcher _sut = null!;
        private Mock<IMailboxSettingsProvider> _mailboxSettingsProviderMock = null!;
        private Mock<ILogger<ImapEmailFetcher>> _loggerMock = null!;
        private MailServerContainer _mailServer = null!;

        public async Task InitializeAsync()
        {
            _fixture = new Fixture();

            _mailboxSettingsProviderMock = new Mock<IMailboxSettingsProvider>();
            _loggerMock = new Mock<ILogger<ImapEmailFetcher>>();

            _sut = new ImapEmailFetcher(
                _mailboxSettingsProviderMock.Object,
                _loggerMock.Object);

            _mailServer = await MailServerContainer.StartNewAsync();
        }

        public async Task DisposeAsync()
        {
            await _mailServer.DisposeAsync();
        }

        [Fact]
        public async Task GetUnreadEmailsAsync_GivenEmptyMailbox_ReturnsEmptyList()
        {
            // GIVEN
            var mailboxId = MailboxId.New();

            _fixture.Customize<ImapSettings>(x => x
                .FromFactory(() => new ImapSettings(
                    _mailServer.Hostname,
                    _mailServer.ImapPort,
                    true,
                    "user",
                    "password")));

            _mailboxSettingsProviderMock
                .Setup(x => x.FindSettingsForMailboxAsync(
                    It.Is<MailboxId>(id => id == mailboxId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<MailboxSettings>());
            
            // WHEN
            var results = await _sut.GetUnreadEmailsAsync(mailboxId, CancellationToken.None);

            // THEN
            results.Should().BeEmpty();
        }
    }
}