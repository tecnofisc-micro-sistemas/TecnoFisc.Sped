using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY570Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY570()
    {
        AssertRegistroEcf.CodesAreImplemented("Y570");
    }

    [Fact]
    public void Parser_LeCnpjCodigoReceitaLosslessIndicadorEValoresOpcionais()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y570|11111111000191|FONTE PAGADORA|S|0916|100000,00|1500,25||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY570>().Which;
        registro.CnpjFon.Should().Be(Cnpj.Create("11111111000191"));
        registro.NomEmp.Should().Be("FONTE PAGADORA");
        registro.IndOrgPub.Should().Be(IndicadorSimNao.Sim);
        registro.CodRec.Should().Be("0916");
        registro.VlRend.Should().Be(100000m);
        registro.IrRet.Should().Be(1500.25m);
        registro.CsllRet.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_CnpjIndicadorEValoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y570|INVALIDO|FONTE|X|5928|BRUTO|IR|CSLL|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY570>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "CNPJ_FON",
                "IND_ORG_PUB",
                "VL_REND",
                "IR_RET",
                "CSLL_RET",
            ]);
    }
}
