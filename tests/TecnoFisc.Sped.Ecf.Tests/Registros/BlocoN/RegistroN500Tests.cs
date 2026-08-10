using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN500Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN500(), "N500", "1:13");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativoSemColidirComCodigoDoRegistro()
    {
        PropertyInfo propriedade = typeof(RegistroN500).GetProperty(nameof(RegistroN500.CampoCodigo))!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be("CODIGO");
        new RegistroN500().Codigo.Should().Be("N500");
    }

    [Fact]
    public void Parser_PreservaCodigoDinamicoDescricaoEValorTextual()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N500|0001|BASE DO IRPJ|-00123,4500|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN500>().Which;
        registro.CampoCodigo.Should().Be("0001");
        registro.Descricao.Should().Be("BASE DO IRPJ");
        registro.Valor.Should().Be("-00123,4500");
    }

    [Fact]
    public void Parser_CamposOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|N500|0001|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN500>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
