using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL200Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL200(), "L200", "0:13");
    }

    [Theory]
    [InlineData("1", MetodoAvaliacaoEstoque.CustoMedioPonderado)]
    [InlineData("2", MetodoAvaliacaoEstoque.Peps)]
    [InlineData("3", MetodoAvaliacaoEstoque.Arbitramento)]
    [InlineData("4", MetodoAvaliacaoEstoque.CustoEspecifico)]
    [InlineData("5", MetodoAvaliacaoEstoque.ValorRealizavelLiquido)]
    [InlineData("6", MetodoAvaliacaoEstoque.InventarioPeriodico)]
    [InlineData("7", MetodoAvaliacaoEstoque.Outros)]
    [InlineData("8", MetodoAvaliacaoEstoque.NaoHa)]
    public void Parser_LeDominioFechadoDoMetodoDeAvaliacao(
        string valor,
        MetodoAvaliacaoEstoque esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|L200|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL200>()
            .Which.IndAvalEstoq.Should().Be(esperado);
    }

    [Fact]
    public void Parser_MetodoForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|L200|9|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL200>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroL200.IndAvalEstoq) && erro.ValorBruto == "9");
    }
}
