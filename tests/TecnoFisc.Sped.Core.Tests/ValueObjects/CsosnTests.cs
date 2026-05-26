namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CsosnTests
{
    [Theory]
    [InlineData("101")]
    [InlineData("500")]
    [InlineData("900")]
    public void Criar_ComTresDigitos_RetornaCsosn(string codigo)
    {
        Csosn.Create(codigo).ToString().Should().Be(codigo);
    }

    [Theory]
    [InlineData("10")]    // 2 dígitos
    [InlineData("1010")]  // 4 dígitos
    [InlineData("10A")]   // não-dígito
    public void Criar_ComTamanhoOuCharInvalido_LancaFormatException(string entrada)
    {
        var act = () => Csosn.Create(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Equality_PorValor()
    {
        Csosn.Create("102").Should().Be(Csosn.Create("102"));
        (Csosn.Create("102") == Csosn.Create("500")).Should().BeFalse();
    }

    [Fact]
    public void Default_ToString_RetornaVazio()
    {
        default(Csosn).ToString().Should().Be(string.Empty);
    }
}
