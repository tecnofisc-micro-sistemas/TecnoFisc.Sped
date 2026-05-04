namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CfopTests
{
    [Theory]
    [InlineData("1102", true, false)]
    [InlineData("2102", true, false)]
    [InlineData("3101", true, false)]
    [InlineData("5102", false, true)]
    [InlineData("6101", false, true)]
    [InlineData("7101", false, true)]
    public void Criar_ComCodigoValido_ClassificaEntradaOuSaida(string codigo, bool entrada, bool saida)
    {
        var cfop = Cfop.Criar(codigo);

        cfop.ToString().Should().Be(codigo);
        cfop.EhEntrada.Should().Be(entrada);
        cfop.EhSaida.Should().Be(saida);
    }

    [Theory]
    [InlineData("4102")]   // primeiro dígito proibido
    [InlineData("0102")]
    [InlineData("8102")]
    [InlineData("9102")]
    [InlineData("510")]    // 3 dígitos
    [InlineData("51020")]  // 5 dígitos
    [InlineData("510A")]   // não-dígito
    [InlineData("")]
    public void Criar_ComCodigoInvalido_LancaFormatException(string codigo)
    {
        var act = () => Cfop.Criar(codigo);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Default_ToString_RetornaQuatroZeros()
    {
        Cfop cfop = default;

        cfop.ToString().Should().Be("0000");
        cfop.EhEntrada.Should().BeFalse();
        cfop.EhSaida.Should().BeFalse();
    }
}
