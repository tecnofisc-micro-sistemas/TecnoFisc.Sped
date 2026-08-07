using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote2;

public sealed class RegistroN660Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN660(), "N660", "1:N");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativo()
    {
        PropertyInfo propriedade = typeof(RegistroN660).GetProperty(nameof(RegistroN660.CampoCodigo))!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be("CODIGO");
    }

    [Fact]
    public void Parser_LeCodigoComZerosDescricaoEValorDecimalComSinal()
    {
        var resultado = new ParserEcf().ParseLinha("|N660|0019|CSLL A PAGAR|-10000,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN660>().Which;
        registro.CampoCodigo.Should().Be("0019");
        registro.Descricao.Should().Be("CSLL A PAGAR");
        registro.Valor.Should().Be(-10000.25m);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|N660|0019|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN660>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }

    [Fact]
    public void Parser_ValorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N660|0019|CSLL A PAGAR|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN660>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "VALOR" && erro.ValorBruto == "INVALIDO");
    }
}
