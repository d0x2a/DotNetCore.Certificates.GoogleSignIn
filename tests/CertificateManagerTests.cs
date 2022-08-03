using Moq;

namespace DotNetCore.Certificates.GoogleSignIn.Tests;

public class CertificateManagerTests
{
    [Fact]
    public async Task TestLoad()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        var manager = new CertificateManager(factory.Object);
        var values = await manager.GetCertificates();
        var valuesCached = await manager.GetCertificates();
        Assert.True(values.Count > 0);
        Assert.Equal(values, valuesCached);
    }
}