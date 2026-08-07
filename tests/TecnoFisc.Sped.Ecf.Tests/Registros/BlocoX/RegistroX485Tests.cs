using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX485Tests
{
    [Fact]
    public void Registro_ConformeManifestoCorrigidoComOnzeCampos()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX485(), "X485", "0:N");
        typeof(RegistroX485).GetProperty(nameof(RegistroX485.CnpjIncorp))!
            .PropertyType.Should().Be<Cnpj?>();
    }

    [Theory]
    [InlineData(nameof(RegistroX485.DtDouPortCebas))]
    [InlineData(nameof(RegistroX485.DtIniPortCebas))]
    [InlineData(nameof(RegistroX485.DtFinPortCebas))]
    public void DatasCebas_UsamDateOnlyNullableEFormatoExato(string nomePropriedade)
    {
        PropertyInfo propriedade = typeof(RegistroX485).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be<DateOnly?>();
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().BeFalse();
    }

    [Fact]
    public void Parser_LeDominioDocumentoCodigosLongosPortariaEDatas()
    {
        const string ato = "ATO DECLARATORIO EXECUTIVO COM IDENTIFICADOR MUITO LONGO 000123";
        var resultado = new ParserEcf().ParseLinha(
            $"|X485|12|{ato}|11222333000181|000000000000000123|000000000000000456|000000000000000789|123/2025|06012025|01012025|31122025|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX485>().Which;
        registro.TipoBenef.Should().Be(TipoBeneficioFiscal.EntidadeBeneficente);
        registro.AtoDecl.Should().Be(ato);
        registro.CnpjIncorp.Should().Be(Cnpj.Create("11222333000181"));
        registro.IdObra2018.Should().Be("000000000000000123");
        registro.IdObra2020.Should().Be("000000000000000456");
        registro.IdObraEei.Should().Be("000000000000000789");
        registro.PortCebas.Should().Be("123/2025");
        registro.DtDouPortCebas.Should().Be(new DateOnly(2025, 1, 6));
        registro.DtIniPortCebas.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFinPortCebas.Should().Be(new DateOnly(2025, 12, 31));
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OptionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|X485|1|ADE 1|||||||||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX485>().Which;
        registro.TipoBenef.Should().Be(TipoBeneficioFiscal.Repes);
        registro.CnpjIncorp.Should().BeNull();
        registro.IdObra2018.Should().BeNull();
        registro.IdObra2020.Should().BeNull();
        registro.IdObraEei.Should().BeNull();
        registro.PortCebas.Should().BeNull();
        registro.DtDouPortCebas.Should().BeNull();
        registro.DtIniPortCebas.Should().BeNull();
        registro.DtFinPortCebas.Should().BeNull();
    }

    [Fact]
    public void Parser_DominioCnpjEDatasInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X485|99|ADE|INVALIDO|||||20250106|0101202X|31132025|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX485>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIPO_BENEF",
                "CNPJ_INCORP",
                "DT_DOU_PORT_CEBAS",
                "DT_INI_PORT_CEBAS",
                "DT_FIN_PORT_CEBAS",
            ]);
    }
}
