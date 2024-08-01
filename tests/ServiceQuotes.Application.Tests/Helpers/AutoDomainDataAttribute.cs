using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace ServiceQuotes.Application.Tests.Helpers;
public class AutoDomainDataAttribute : AutoDataAttribute
{
    public AutoDomainDataAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization())) { }
}
