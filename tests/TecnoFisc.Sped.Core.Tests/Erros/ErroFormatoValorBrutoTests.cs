using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Core.Tests.Erros;

public sealed class ErroFormatoValorBrutoTests
{
    [Fact]
    public void ValorBruto_QuandoNaoInformado_EhNull()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido");

        erro.ValorBruto.Should().BeNull();
    }

    [Fact]
    public void ValorBruto_QuandoInformadoViaInit_Preserva()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido") { ValorBruto = "3225...6541" };

        erro.ValorBruto.Should().Be("3225...6541");
    }

    [Fact]
    public void ToString_PermaneceInalterado_IgnorandoValorBruto()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido") { ValorBruto = "X" };

        erro.ToString().Should().Be("Linha 10 (C100.ChvNfe): Valor inválido");
    }
}
