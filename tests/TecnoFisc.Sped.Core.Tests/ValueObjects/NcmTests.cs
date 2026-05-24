namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class NcmTests
{
    [Theory]
    [InlineData("84713019")]
    [InlineData("8471.30.19")]
    public void Criar_ComCodigoValido_RetornaNcm(string entrada)
    {
        var ncm = Ncm.Create(entrada);

        ncm.ToString().Should().Be("84713019");
    }

    [Theory]
    [InlineData("8471301")]    // 7 dígitos
    [InlineData("847130199")]  // 9 dígitos
    [InlineData("8471301X")]
    [InlineData("")]
    public void Criar_ComCodigoInvalido_LancaFormatException(string entrada)
    {
        var act = () => Ncm.Create(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Equals_ComMesmosDigitos_RetornaTrue()
    {
        var a = Ncm.Create("84713019");
        var b = Ncm.Create("8471.30.19");

        a.Should().Be(b);
    }
}
