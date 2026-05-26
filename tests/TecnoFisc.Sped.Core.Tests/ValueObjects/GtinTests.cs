namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class GtinTests
{
    [Theory]
    [InlineData("7891149010400")] // GTIN-13 real
    [InlineData("0000078905351")] // GTIN-13 real com zeros à esquerda
    [InlineData("96385074")]      // GTIN-8
    [InlineData("10614141000415")]// GTIN-14
    public void Criar_ComGtinValido_RetornaGtin(string codigo)
    {
        var gtin = Gtin.Create(codigo);

        gtin.ToString().Should().Be(codigo);
        gtin.IsSemGtin.Should().BeFalse();
    }

    [Fact]
    public void Criar_ComSemGtin_RetornaSentinela()
    {
        var gtin = Gtin.Create("SEM GTIN");

        gtin.IsSemGtin.Should().BeTrue();
        gtin.ToString().Should().Be("SEM GTIN");
    }

    [Fact]
    public void Criar_ComDvErrado_LancaFormatException()
    {
        // GTIN-13 real com o último dígito alterado (DV inválido).
        var act = () => Gtin.Create("7891149010401");

        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("1234567")]        // 7 dígitos (comprimento inválido)
    [InlineData("123456789012345")]// 15 dígitos
    [InlineData("789114901040A")]  // não-dígito
    public void Criar_ComTamanhoOuCharInvalido_LancaFormatException(string entrada)
    {
        var act = () => Gtin.Create(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarCriar_ComValorInvalido_RetornaFalse()
    {
        Gtin.TentarCriar("7891149010401", out _).Should().BeFalse();
    }

    [Fact]
    public void Equality_PorValor()
    {
        Gtin.Create("7891149010400").Should().Be(Gtin.Create("7891149010400"));
        (Gtin.Create("7891149010400") != Gtin.Create("0000078905351")).Should().BeTrue();
    }

    [Fact]
    public void Default_ToString_RetornaSentinela()
    {
        default(Gtin).ToString().Should().Be("SEM GTIN");
    }
}
