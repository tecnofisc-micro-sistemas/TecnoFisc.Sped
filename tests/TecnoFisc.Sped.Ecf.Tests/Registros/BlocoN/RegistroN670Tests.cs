using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote2;

public sealed class RegistroN670Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN670(), "N670", "0:N");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativo()
    {
        PropertyInfo propriedade = typeof(RegistroN670).GetProperty(nameof(RegistroN670.CampoCodigo))!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be("CODIGO");
    }

    [Fact]
    public void Parser_LeCodigoComZerosDescricaoEValorDecimal()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N670|0023|CSLL POSTERGADA DE PERIODOS ANTERIORES|10000,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN670>().Which;
        registro.CampoCodigo.Should().Be("0023");
        registro.Descricao.Should().Be("CSLL POSTERGADA DE PERIODOS ANTERIORES");
        registro.Valor.Should().Be(10000.25m);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|N670|0023|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN670>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }

    [Fact]
    public void Parser_ValorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N670|0023|CSLL POSTERGADA|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN670>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroN670.Valor) && erro.ValorBruto == "INVALIDO");
    }
}
