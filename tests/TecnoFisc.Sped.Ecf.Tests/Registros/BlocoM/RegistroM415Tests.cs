using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM415Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM415(), "M415", "0:N");
    }

    [Fact]
    public void Parser_LeProcessoJudicialSemNormalizarNumero()
    {
        var resultado = new ParserEcf().ParseLinha("|M415|1|0001111-22.2025.5.01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM415>().Which;
        registro.IndProc.Should().Be(TipoProcessoEcf.Judicial);
        registro.NumProc.Should().Be("0001111-22.2025.5.01");
    }

    [Fact]
    public void Parser_TipoDeProcessoForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M415|0|0001|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM415>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IND_PROC" && erro.ValorBruto == "0");
    }
}
