using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY612Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY612()
    {
        AssertRegistroEcf.CodesAreImplemented("Y612");
    }

    [Fact]
    public void DominioFechado_CoincideComTabelaCompletaDoManual()
    {
        typeof(QualificacaoDirigenteConselheiro)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("10", "11", "12", "13", "14", "15", "16", "17");
    }

    [Fact]
    public void Parser_LeCpfQualificacaoFechadaEValores()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y612|52998224725|DIRIGENTE TESTE|12|50000,00|-10000,25|8000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY612>().Which;
        registro.Cpf.Should().Be(Cpf.Create("52998224725"));
        registro.Nome.Should().Be("DIRIGENTE TESTE");
        registro.Qualif.Should().Be(QualificacaoDirigenteConselheiro.PresidenteSemVinculo);
        registro.VlRemTrab.Should().Be(50000m);
        registro.VlDemRend.Should().Be(-10000.25m);
        registro.VlIrRet.Should().Be(8000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_CpfQualificacaoEValoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y612|INVALIDO|DIRIGENTE|01|TRAB|REND|IR|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY612>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "CPF",
                "QUALIF",
                "VL_REM_TRAB",
                "VL_DEM_REND",
                "VL_IR_RET",
            ]);
    }
}
