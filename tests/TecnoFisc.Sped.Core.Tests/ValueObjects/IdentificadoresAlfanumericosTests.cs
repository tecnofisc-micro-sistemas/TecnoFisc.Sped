namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class IdentificadoresAlfanumericosTests
{
    [Theory]
    [InlineData("12ABC34501DE35")]
    [InlineData("12.ABC.345/01DE-35")]
    public void OfficialCnpjExampleIsAccepted(string value)
    {
        Cnpj.Create(value).ToString().Should().Be("12ABC34501DE35");
    }

    [Theory]
    [InlineData("12ABC34501DE36")]
    [InlineData("12ABC34501DE3A")]
    [InlineData("12ÁBC34501DE35")]
    public void InvalidCheckDigitOrAlphabetIsRejected(string value)
        => Cnpj.TentarCriar(value, out _).Should().BeFalse();

    [Fact]
    public void AccessKeySupportsAlphanumericIssuer()
    {
        // Fixed vector, ASCII-48 / modulo 11 per NT conjunta 2025.001, annex II.
        const string key = "35261012ABC34501DE35550010000000011000000011";
        var result = ChaveAcesso.Create(key);
        result.CnpjEmitente.ToString().Should().Be("12ABC34501DE35");
        result.Modelo.Should().Be(55);
    }

    [Theory]
    [InlineData("35261000052998224725559200000000011000000018")]
    [InlineData("35261000052998224725558900000000011000000011")]
    [InlineData("35261000052998224725559100000000011000000012")]
    [InlineData("35261000052998224725550260210200013000000017")]
    public void AccessKeySupportsCpfIssuerWithLeftPadding(string key)
    {
        var result = ChaveAcesso.Create(key);
        result.CpfEmitente!.Value.ToString().Should().Be("52998224725");
    }

    [Theory]
    [InlineData("35261000052998224725550010000000013000000019")]
    [InlineData("35261000052998224725550010000000011000000012")]
    public void CpfIsNotUsedToRescueInvalidCnpjInOrdinarySeries(string key)
        => ChaveAcesso.TentarCriar(key, out _).Should().BeFalse();

    [Fact]
    public void ExistingNffCpfVectorHasValidCheckDigit()
    {
        var key = ChaveAcesso.Create("32251100011998216756550252411200013640116541");
        key.CpfEmitente.Should().NotBeNull();
        key.TipoEmissao.Should().Be(3);
    }
}
