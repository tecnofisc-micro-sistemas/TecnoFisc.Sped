using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN610Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN610(), "N610", "1:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorDaTabelaDinamica()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N610|0077|REDUCAO POR REINVESTIMENTO|10000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN610>().Which;
        registro.CampoCodigo.Should().Be("0077");
        registro.Descricao.Should().Be("REDUCAO POR REINVESTIMENTO");
        registro.Valor.Should().Be("10000,00");
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|N610|0003|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN610>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
