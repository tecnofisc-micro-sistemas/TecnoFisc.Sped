using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX280Tests
{
    private const string LinhaCompleta =
        "|X280|01|AM|01|ATO-0001|01012021|31122026|11222333000181|00001234|100000,50|25000,25|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX280(), "X280", "0:N");
        typeof(RegistroX280).GetProperty(nameof(RegistroX280.CnpjIncentivo))!
            .PropertyType.Should().Be<Cnpj>();
    }

    [Theory]
    [InlineData(nameof(RegistroX280.VigIni))]
    [InlineData(nameof(RegistroX280.VigFim))]
    public void VigenciasLexicaisD8_UsamDateOnlyEFormatoExato(string propriedade)
    {
        PropertyInfo info = typeof(RegistroX280).GetProperty(propriedade)!;
        CampoSpedAttribute campo = info.GetCustomAttribute<CampoSpedAttribute>()!;

        info.PropertyType.Should().Be<DateOnly>();
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Parser_LeDominiosDatasDocumentosECodigosSemPerda()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX280>().Which;
        registro.IndAtiv.Should().Be(BeneficioFiscalIncentivado.Isencao);
        registro.IndConcedente.Should().Be(OrgaoConcedenteIncentivo.Sudam);
        registro.IndProj.Should().Be(ProjetoIncentivado.NovoEmpreendimento);
        registro.AtoConc.Should().Be("ATO-0001");
        registro.VigIni.Should().Be(new DateOnly(2021, 1, 1));
        registro.VigFim.Should().Be(new DateOnly(2026, 12, 31));
        registro.CnpjIncentivo.Should().Be(Cnpj.Create("11222333000181"));
        registro.NcmIncentivo.Should().Be("00001234");
        registro.RecLiqIncentivo.Should().Be(100000.50m);
        registro.VlIncentivo.Should().Be(25000.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X280|99|OU|99|Outros|01012025|31122025|11222333000181||0,00||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX280>().Which;
        registro.NcmIncentivo.Should().BeNull();
        registro.VlIncentivo.Should().BeNull();
        registro.CnpjIncentivo.Should().Be(Cnpj.Create("11222333000181"));
    }

    [Fact]
    public void Parser_DominiosDatasEDecimaisInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X280|XX|ZZ|YY|ATO|20210101|31132026|INVALIDO|00001234|INVALIDO|OUTRO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX280>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroX280.IndAtiv),
                nameof(RegistroX280.IndConcedente),
                nameof(RegistroX280.IndProj),
                nameof(RegistroX280.VigIni),
                nameof(RegistroX280.VigFim),
                nameof(RegistroX280.CnpjIncentivo),
                nameof(RegistroX280.RecLiqIncentivo),
                nameof(RegistroX280.VlIncentivo),
            ]);
    }
}
