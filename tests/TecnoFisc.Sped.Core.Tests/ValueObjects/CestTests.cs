namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CestTests
{
    [Fact]
    public void Criar_ComSeteDigitos_RetornaCest()
    {
        var cest = Cest.Create("0302100");

        cest.ToString().Should().Be("0302100");
    }

    [Fact]
    public void Criar_AceitaMascaraComPontos()
    {
        var cest = Cest.Create("03.021.00");

        cest.ToString().Should().Be("0302100");
    }

    [Theory]
    [InlineData("030210")]    // 6 dígitos
    [InlineData("03021000")]  // 8 dígitos
    [InlineData("030210A")]   // não-dígito
    public void Criar_ComTamanhoOuCharInvalido_LancaFormatException(string entrada)
    {
        var act = () => Cest.Create(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarCriar_ComValorInvalido_RetornaFalse()
    {
        Cest.TentarCriar("xxx", out _).Should().BeFalse();
    }

    [Fact]
    public void Equality_PorValor()
    {
        Cest.Create("0302100").Should().Be(Cest.Create("0302100"));
        (Cest.Create("0302100") == Cest.Create("0301100")).Should().BeFalse();
    }

    [Fact]
    public void Default_ToString_RetornaZeros()
    {
        default(Cest).ToString().Should().Be("0000000");
    }
}
