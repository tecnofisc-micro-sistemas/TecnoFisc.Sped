using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.217 - exercita a forma do <see cref="Registro1100"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 269-270): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1100).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_Declara1100_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1100ComOnzeCamposNaOrdem()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1100");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndDoc",
            "NroDe",
            "DtDe",
            "NatExp",
            "NroRe",
            "DtRe",
            "ChcEmb",
            "DtChc",
            "DtAvb",
            "TpChc",
            "Pais",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 11));
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[4].Obrigatorio.Should().BeFalse();
        meta.Campos[8].Obrigatorio.Should().BeTrue();
        meta.Campos[9].Obrigatorio.Should().BeTrue();
        meta.Campos[10].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());
        meta.Campos[1].Definidor(registro, "20240012345678".AsSpan());
        meta.Campos[2].Definidor(registro, "15032024".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());
        meta.Campos[4].Definidor(registro, "123456789012".AsSpan());
        meta.Campos[5].Definidor(registro, "16032024".AsSpan());
        meta.Campos[6].Definidor(registro, "CHC123456789".AsSpan());
        meta.Campos[7].Definidor(registro, "17032024".AsSpan());
        meta.Campos[8].Definidor(registro, "18032024".AsSpan());
        meta.Campos[9].Definidor(registro, "10".AsSpan());
        meta.Campos[10].Definidor(registro, "063".AsSpan());

        registro.IndDoc.Should().Be(IndicadorDocumentoExportacao.DeclaracaoExportacao);
        registro.NroDe.Should().Be("20240012345678");
        registro.DtDe.Should().Be(new DateOnly(2024, 3, 15));
        registro.NatExp.Should().Be(NaturezaExportacao.Indireta);
        registro.NroRe.Should().Be(123456789012);
        registro.DtRe.Should().Be(new DateOnly(2024, 3, 16));
        registro.ChcEmb.Should().Be("CHC123456789");
        registro.DtChc.Should().Be(new DateOnly(2024, 3, 17));
        registro.DtAvb.Should().Be(new DateOnly(2024, 3, 18));
        registro.TpChc.Should().Be(TipoConhecimentoEmbarque.Bl);
        registro.Pais.Should().Be("063");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, Span<char>.Empty);
        meta.Campos[5].Definidor(registro, Span<char>.Empty);
        meta.Campos[6].Definidor(registro, Span<char>.Empty);
        meta.Campos[7].Definidor(registro, Span<char>.Empty);

        registro.NroRe.Should().BeNull();
        registro.DtRe.Should().BeNull();
        registro.ChcEmb.Should().BeNull();
        registro.DtChc.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorDocumentoExportacao.DeclaracaoExportacao)]
    [InlineData("1", IndicadorDocumentoExportacao.DeclaracaoSimplificadaExportacao)]
    [InlineData("2", IndicadorDocumentoExportacao.DeclaracaoUnicaExportacao)]
    public void Definidor_IndDoc_MapeiaTodosOsValores(string valor, IndicadorDocumentoExportacao esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndDoc.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", NaturezaExportacao.Direta)]
    [InlineData("1", NaturezaExportacao.Indireta)]
    public void Definidor_NatExp_MapeiaTodosOsValores(string valor, NaturezaExportacao esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, valor.AsSpan());

        registro.NatExp.Should().Be(esperado);
    }

    [Theory]
    [InlineData("01", TipoConhecimentoEmbarque.Awb)]
    [InlineData("02", TipoConhecimentoEmbarque.Mawb)]
    [InlineData("03", TipoConhecimentoEmbarque.Hawb)]
    [InlineData("04", TipoConhecimentoEmbarque.Comat)]
    [InlineData("06", TipoConhecimentoEmbarque.RExpressas)]
    [InlineData("07", TipoConhecimentoEmbarque.EtiqRexpressas)]
    [InlineData("08", TipoConhecimentoEmbarque.HrExpressas)]
    [InlineData("09", TipoConhecimentoEmbarque.Av7)]
    [InlineData("10", TipoConhecimentoEmbarque.Bl)]
    [InlineData("11", TipoConhecimentoEmbarque.Mbl)]
    [InlineData("12", TipoConhecimentoEmbarque.Hbl)]
    [InlineData("13", TipoConhecimentoEmbarque.Crt)]
    [InlineData("14", TipoConhecimentoEmbarque.Dsic)]
    [InlineData("16", TipoConhecimentoEmbarque.ComatBl)]
    [InlineData("17", TipoConhecimentoEmbarque.Rwb)]
    [InlineData("18", TipoConhecimentoEmbarque.Hrwb)]
    [InlineData("19", TipoConhecimentoEmbarque.TifDta)]
    [InlineData("20", TipoConhecimentoEmbarque.Cp2)]
    [InlineData("91", TipoConhecimentoEmbarque.NaoIata)]
    [InlineData("92", TipoConhecimentoEmbarque.MNaoIata)]
    [InlineData("93", TipoConhecimentoEmbarque.HNaoIata)]
    [InlineData("99", TipoConhecimentoEmbarque.Outros)]
    public void Definidor_TpChc_MapeiaTodosOsValores(string valor, TipoConhecimentoEmbarque esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[9].Definidor(registro, valor.AsSpan());

        registro.TpChc.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorDocumentoExportacao.DeclaracaoExportacao, "0")]
    [InlineData(IndicadorDocumentoExportacao.DeclaracaoSimplificadaExportacao, "1")]
    [InlineData(IndicadorDocumentoExportacao.DeclaracaoUnicaExportacao, "2")]
    public void Serializar_IndDoc_RetornaCodigo(IndicadorDocumentoExportacao indicador, string esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();
        registro.IndDoc = indicador;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(TipoConhecimentoEmbarque.Awb, "01")]
    [InlineData(TipoConhecimentoEmbarque.Outros, "99")]
    public void Serializar_TpChc_RetornaCodigoComDoisDigitos(TipoConhecimentoEmbarque tipo, string esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();
        registro.TpChc = tipo;

        meta.Campos[9].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1100|0|20240012345678|15032024|1|123456789012|16032024|CHC123456789|17032024|18032024|10|063|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DeclaracaoUnicaSemRegistroExportacao_PreservaTextoCanonico()
    {
        const string sped = "|1100|2|DU202400000001|20042024|0|||||21042024|99|249|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
