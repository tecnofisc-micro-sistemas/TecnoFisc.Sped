using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote4;

public sealed class RegistroX500Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX500(), "X500", "0:N");
    }

    [Fact]
    public void Parser_PreservaCamposDinamicosSemLimiteFixo()
    {
        const string codigo = "ZPE-CODIGO-SEM-LIMITE-0001";
        const string descricao = "LINHA DINAMICA DA ZPE";
        const string valor = "000123,45 TEXTO LIVRE";
        var resultado = new ParserEcf().ParseLinha($"|X500|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX500>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|X500|ZPE-01|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX500>().Which;
        registro.CampoCodigo.Should().Be("ZPE-01");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
