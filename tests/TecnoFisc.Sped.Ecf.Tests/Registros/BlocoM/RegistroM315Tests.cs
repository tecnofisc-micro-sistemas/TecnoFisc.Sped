using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM315Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM315(), "M315", "0:N");
    }

    [Theory]
    [InlineData("1", TipoProcessoEcf.Judicial)]
    [InlineData("2", TipoProcessoEcf.Administrativo)]
    public void Parser_LeTipoEPreservaNumeroDoProcesso(
        string valor,
        TipoProcessoEcf esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|M315|{valor}|0001234-56.2025.4.01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM315>().Which;
        registro.IndProc.Should().Be(esperado);
        registro.NumProc.Should().Be("0001234-56.2025.4.01");
    }

    [Fact]
    public void Parser_TipoDeProcessoForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M315|3|0001|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM315>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IND_PROC" && erro.ValorBruto == "3");
    }
}
