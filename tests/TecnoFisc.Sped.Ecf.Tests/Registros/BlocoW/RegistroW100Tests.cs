using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoW;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoW;

public sealed class RegistroW100Tests
{
    private const string LinhaCompleta =
        "|W100|GRUPO GLOBAL|N|CONTROLADORA FINAL|DE|DE-TIN-000042|4|1|" +
        "ENTIDADE SUBSTITUTA|US|US-EIN-A9|01012025|31122025|EUR|PT|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroW100(), "W100", "0:1");
    }

    [Fact]
    public void Parser_LeDeclaracaoComTinLosslessDatasEDominiosRevisados()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW100>().Which;
        registro.NomeMultinacional.Should().Be("GRUPO GLOBAL");
        registro.IndControladora.Should().Be(IndicadorSimNao.Nao);
        registro.NomeControladora.Should().Be("CONTROLADORA FINAL");
        registro.JurisdicaoControladora.Should().Be("DE");
        registro.TinControladora.Should().Be("DE-TIN-000042");
        registro.IndEntrega.Should().Be(ResponsavelEntregaDpp.OutraEntidade);
        registro.IndModalidade.Should().Be(ModalidadeEntregaDpp.EntidadeSubstituta);
        registro.NomeSubstituta.Should().Be("ENTIDADE SUBSTITUTA");
        registro.JurisdicaoSubstituta.Should().Be("US");
        registro.TinSubstituta.Should().Be("US-EIN-A9");
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 12, 31));
        registro.TipMoeda.Should().Be("EUR");
        registro.IndIdioma.Should().Be(IdiomaDpp.Portugues);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("NOTIN", "NOTIN")]
    [InlineData("12345678000195", "12345678000195")]
    [InlineData("TIN-US-A9", "TIN-US-A9")]
    public void Parser_PreservaTinGenericoSemInferirCnpj(string tin, string esperado)
    {
        string linha = $"|W100|GRUPO|S|CONTROLADORA|ZZ|{tin}|2|2||||01012025|31122025|ZZZ|EN|";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW100>().Which;
        registro.TinControladora.Should().Be(esperado);
        registro.JurisdicaoControladora.Should().Be("ZZ");
        registro.TipMoeda.Should().Be("ZZZ");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("X", "1", "PT", "IND_CONTROLADORA")]
    [InlineData("S", "9", "PT", "IND_MODALIDADE")]
    [InlineData("S", "1", "XX", "IND_IDIOMA")]
    public void Parser_DominioInvalido_RegistraErroDeFormato(
        string controladora,
        string modalidade,
        string idioma,
        string campo)
    {
        string linha =
            $"|W100|GRUPO|{controladora}|CONTROLADORA|DE|TIN|4|{modalidade}|" +
            $"SUBSTITUTA|US|TIN-SUB|01012025|31122025|EUR|{idioma}|";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW100>()
            .Which.ErrosDeFormato.Should().Contain(erro => erro.Campo == campo);
    }

    [Fact]
    public void Parser_DataInvalida_RegistraErroSemExecutarRegraDoPeriodo()
    {
        string linha = LinhaCompleta.Replace("01012025", "31132025", StringComparison.Ordinal);

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW100>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "DT_INI" && erro.ValorBruto == "31132025");
    }
}
