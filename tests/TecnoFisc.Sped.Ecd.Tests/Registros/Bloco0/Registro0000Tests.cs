using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 10.001 — exercita a forma do <see cref="Registro0000"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 64): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core (não há gerador específico da ECD).
/// </summary>
public sealed class Registro0000Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_Declara0000_Nivel0_Bloco0()
    {
        var atributo = typeof(Registro0000).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0000");
        atributo.Nivel.Should().Be(0);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void VersaoLeiaute_RetornaLeiaute9()
    {
        var registro = new Registro0000();

        registro.VersaoLeiaute.Should().Be((int)LayoutEcd.V009);
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0000ComVinteEDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0000");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Lecd",
            "DtIni",
            "DtFin",
            "Nome",
            "Cnpj",
            "Uf",
            "Ie",
            "CodMun",
            "Im",
            "IndSitEsp",
            "IndSitIniPer",
            "IndNire",
            "IndFinEsc",
            "CodHashSub",
            "IndGrandePorte",
            "TipEcd",
            "CodScp",
            "IdentMf",
            "IndEscCons",
            "IndCentralizada",
            "IndMudancPc",
            "CodPlanRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "LECD".AsSpan());
        meta.Campos[1].Definidor(registro, "01012023".AsSpan());
        meta.Campos[2].Definidor(registro, "31122023".AsSpan());
        meta.Campos[3].Definidor(registro, "EMPRESA TESTE SA".AsSpan());
        meta.Campos[4].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[5].Definidor(registro, "SP".AsSpan());
        meta.Campos[6].Definidor(registro, "123456789".AsSpan());
        meta.Campos[7].Definidor(registro, "3550308".AsSpan());
        meta.Campos[8].Definidor(registro, "99999".AsSpan());
        meta.Campos[9].Definidor(registro, "1".AsSpan());
        meta.Campos[10].Definidor(registro, "0".AsSpan());
        meta.Campos[11].Definidor(registro, "1".AsSpan());
        meta.Campos[12].Definidor(registro, "1".AsSpan());
        meta.Campos[13].Definidor(registro, "ABCDEF0123456789ABCDEF0123456789ABCDEF01".AsSpan());
        meta.Campos[14].Definidor(registro, "1".AsSpan());
        meta.Campos[15].Definidor(registro, "2".AsSpan());
        meta.Campos[16].Definidor(registro, "11444777000161".AsSpan());
        meta.Campos[17].Definidor(registro, "S".AsSpan());
        meta.Campos[18].Definidor(registro, "S".AsSpan());
        meta.Campos[19].Definidor(registro, "0".AsSpan());
        meta.Campos[20].Definidor(registro, "0".AsSpan());
        meta.Campos[21].Definidor(registro, "11".AsSpan());

        registro.Lecd.Should().Be("LECD");
        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2023, 12, 31));
        registro.Nome.Should().Be("EMPRESA TESTE SA");
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        registro.Uf.Should().Be("SP");
        registro.Ie.Should().Be("123456789");
        registro.CodMun.Should().Be("3550308");
        registro.Im.Should().Be("99999");
        registro.IndSitEsp.Should().Be(SituacaoEspecial.Cisao);
        registro.IndSitIniPer.Should().Be(SituacaoInicioPeriodo.Normal);
        registro.IndNire.Should().Be(IndicadorExistenciaNire.Possui);
        registro.IndFinEsc.Should().Be(IndicadorFinalidadeEscrituracao.Substituta);
        registro.CodHashSub.Should().Be("ABCDEF0123456789ABCDEF0123456789ABCDEF01");
        registro.IndGrandePorte.Should().Be(IndicadorGrandePorte.SujeitaAuditoria);
        registro.TipEcd.Should().Be(TipoEcd.Scp);
        registro.CodScp.Should().Be(Cnpj.Create("11444777000161"));
        registro.IdentMf.Should().Be(IndicadorSimNao.Sim);
        registro.IndEscCons.Should().Be(IndicadorSimNao.Sim);
        registro.IndCentralizada.Should().Be(IndicadorEscrituracaoCentralizada.Centralizada);
        registro.IndMudancPc.Should().Be(IndicadorMudancaPlanoContas.SemMudanca);
        registro.CodPlanRef.Should().Be(CodigoPlanoContasReferencial.TributacaoEspecificaFutebol);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();

        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // Ie
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodMun
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // Im
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndSitEsp
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // CodHashSub
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // CodScp
        meta.Campos[21].Definidor(registro, ReadOnlySpan<char>.Empty); // CodPlanRef

        registro.Ie.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Im.Should().BeNull();
        registro.IndSitEsp.Should().BeNull();
        registro.CodHashSub.Should().BeNull();
        registro.CodScp.Should().BeNull();
        registro.CodPlanRef.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorFinalidadeEscrituracao.Original, "0")]
    [InlineData(IndicadorFinalidadeEscrituracao.Substituta, "1")]
    public void Serializar_IndFinEsc_RetornaCodigo(IndicadorFinalidadeEscrituracao finalidade, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndFinEsc = finalidade;

        meta.Campos[12].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorSimNao.Nao, "N")]
    [InlineData(IndicadorSimNao.Sim, "S")]
    public void Serializar_IdentMf_RetornaCodigo(IndicadorSimNao identificacao, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IdentMf = identificacao;

        meta.Campos[17].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|LECD|01012023|31122023|EMPRESA TESTE SA|11222333000181|SP|123456789|3550308|99999|1|0|1|1|ABCDEF0123456789ABCDEF0123456789ABCDEF01|1|2|11444777000161|S|S|0|0|11|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|LECD|01012023|31122023|JOAO SILVA ME|11222333000181|MG|||||0|1|0||0|0||N|N|1|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
