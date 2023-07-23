using AutoFixture;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
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
        private IContainer _greenMailContainer = null!;

        public async Task InitializeAsync()
        {
            _fixture = new Fixture();

            _mailboxSettingsProviderMock = new Mock<IMailboxSettingsProvider>();
            _loggerMock = new Mock<ILogger<ImapEmailFetcher>>();

            _sut = new ImapEmailFetcher(
                _mailboxSettingsProviderMock.Object,
                _loggerMock.Object);

            _greenMailContainer = new ContainerBuilder()
                .WithDockerEndpoint("tcp://localhost:2375")
                .WithImage("greenmail/standalone:latest")
                .WithPortBinding(3025, 3143)
                .Build();
            await _greenMailContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _greenMailContainer.StopAsync();
        }

        [Fact]
        public async Task GetUnreadEmailsAsync_GivenEmptyMailbox_ReturnsEmptyList()
        {
            // GIVEN
            var mailboxId = MailboxId.New();

            _fixture.Customize<ImapSettings>(x => x
                .FromFactory(() => new ImapSettings(
                    _greenMailContainer.Hostname,
                    3025,
                    true,
                    "user",
                    "password")));

            _mailboxSettingsProviderMock
                .Setup(x => x.FindSettingsForMailboxAsync(
                    It.Is(mailboxId, MailboxId.EqualityComparer),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<MailboxSettings>());

            // WHEN
            var results = await _sut.GetUnreadEmailsAsync(mailboxId, CancellationToken.None);

            // THEN
            results.Should().BeEmpty();
        }
    }
}