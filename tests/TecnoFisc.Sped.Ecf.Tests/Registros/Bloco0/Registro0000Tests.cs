using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0000Tests
{
    private const string LinhaCompleta =
        "|0000|LECF|0011|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||";

    [Fact]
    public void AtributoECampos_RefletemManifestoRevisadoDoLeiaute12()
    {
        var atributoRegistro = typeof(Registro0000).GetCustomAttribute<RegistroSpedAttribute>();
        var propriedades = typeof(Registro0000)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(propriedade => (Propriedade: propriedade, Campo: propriedade.GetCustomAttribute<CampoSpedAttribute>()))
            .Where(item => item.Campo is not null)
            .OrderBy(item => item.Campo!.Ordem)
            .ToArray();

        atributoRegistro.Should().NotBeNull();
        atributoRegistro!.Codigo.Should().Be("0000");
        atributoRegistro.Nivel.Should().Be(0);
        atributoRegistro.Bloco.Should().Be("0");

        propriedades.Select(item => item.Propriedade.Name).Should().Equal(
        [
            "NomeEsc", "CodVer", "Cnpj", "Nome", "IndSitIniPer", "SitEspecial",
            "PatRemanCis", "DtSitEsp", "DtIni", "DtFin", "Retificadora", "NumRec",
            "TipEcf", "CodScp",
        ]);
        propriedades.Select(item => item.Campo!.Ordem).Should().Equal(Enumerable.Range(2, 14));
        propriedades.Select(item => item.Campo!.Tamanho).Should().Equal(
            [4, 4, 14, 0, 1, 1, 8, 8, 8, 8, 1, 40, 1, 14]);
        propriedades.Select(item => item.Campo!.Decimais).Should().Equal(
            [0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0]);
        propriedades.Select(item => item.Campo!.Obrigatorio).Should().Equal(
            [true, true, true, true, true, true, false, false, true, true, true, false, true, false]);
        propriedades.Select(item => item.Campo!.Formato).Should().Equal(
            [null, null, null, null, null, null, null, "ddMMyyyy", "ddMMyyyy", "ddMMyyyy", null, null, null, null]);
    }

    [Theory]
    [InlineData("0008", 8)]
    [InlineData("0009", 9)]
    [InlineData("0010", 10)]
    [InlineData("0011", 11)]
    [InlineData("0012", 12)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData(" ", 0)]
    [InlineData("ABCD", 0)]
    public void VersaoLeiaute_ConverteCodVer(string? codVer, int esperado)
    {
        var registro = new Registro0000 { CodVer = codVer };

        registro.VersaoLeiaute.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0008", 8)]
    [InlineData("0012", 12)]
    [InlineData("0013", 13)]
    [InlineData("0007", 7)]
    [InlineData("0100", 100)]
    public void VersaoLeiaute_ParseiaCodVerNumericamente(string codVer, int esperado)
        => new Registro0000 { CodVer = codVer }.VersaoLeiaute.Should().Be(esperado);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABCD")]
    [InlineData("00 1")]
    public void VersaoLeiaute_EhZeroQuandoCodVerNaoEhNumerico(string? codVer)
        => new Registro0000 { CodVer = codVer }.VersaoLeiaute.Should().Be(0);

    [Fact]
    public void ParserPadrao_CatalogoGeradoResolve0000()
    {
        var parser = new ParserEcf();

        var resultado = parser.ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro0000>();
        var registro = (Registro0000)resultado.Valor!;
        registro.NomeEsc.Should().Be("LECF");
        registro.CodVer.Should().Be("0011");
        registro.Cnpj.Should().Be(Cnpj.Create("11111111000191"));
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 12, 31));
        registro.VersaoLeiaute.Should().Be(11);
    }
}
