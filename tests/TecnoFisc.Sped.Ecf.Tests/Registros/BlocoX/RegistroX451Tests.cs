using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX451Tests
{
    [Fact]
    public void Registro_ConformeManifestoComoFilhoNivelTres()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX451(), "X451", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoComZerosEDinamicosLongosSemNormalizar()
    {
        const string descricao = "DESCRICAO DINAMICA MUITO LONGA SEM LIMITE FIXO";
        const string valor = "R$ -1.234,56 / VALOR LIVRE";
        var resultado = new ParserEcf().ParseLinha($"|X451|000001|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX451>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OptionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|X451|000002|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX451>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
