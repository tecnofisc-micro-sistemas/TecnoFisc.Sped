using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1010).Assembly);

    [Fact]
    public void Atributo_Declara1010_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1010Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1010");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NumProc", "IdSecJud", "IdVara", "IndNatAcao", "DescDecJud", "DtSentJud",
        ]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumProc
        meta.Campos[1].Tamanho.Should().Be(0);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // IdSecJud
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // IdVara
        meta.Campos[3].Tamanho.Should().Be(2);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // IndNatAcao
        meta.Campos[4].Tamanho.Should().Be(100);
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // DescDecJud
        meta.Campos[5].Tamanho.Should().Be(8);
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DtSentJud
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0001234-56.2020.1.00".AsSpan());              // NumProc
        meta.Campos[1].Definidor(registro, "4JF".AsSpan());                               // IdSecJud
        meta.Campos[2].Definidor(registro, "10".AsSpan());                                // IdVara
        meta.Campos[3].Definidor(registro, "02".AsSpan());                                // IndNatAcao
        meta.Campos[4].Definidor(registro, "Valores com exigibilidade suspensa".AsSpan()); // DescDecJud
        meta.Campos[5].Definidor(registro, "20032019".AsSpan());                          // DtSentJud

        registro.NumProc.Should().Be("0001234-56.2020.1.00");
        registro.IdSecJud.Should().Be("4JF");
        registro.IdVara.Should().Be("10");
        registro.IndNatAcao.Should().Be(IndicadorNaturezaAcaoJudicial.DecisaoNaoTransitadaEmJulgadoFavoravel);
        registro.DescDecJud.Should().Be("Valores com exigibilidade suspensa");
        registro.DtSentJud.Should().Be(new DateOnly(2019, 3, 20));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // DescDecJud
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DtSentJud

        registro.DescDecJud.Should().BeNull();
        registro.DtSentJud.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorNaturezaAcaoJudicial.DecisaoTransitadaEmJulgadoFavoravel)]
    [InlineData("02", IndicadorNaturezaAcaoJudicial.DecisaoNaoTransitadaEmJulgadoFavoravel)]
    [InlineData("03", IndicadorNaturezaAcaoJudicial.LiminarMandadoSeguranca)]
    [InlineData("04", IndicadorNaturezaAcaoJudicial.LiminarMedidaCautelar)]
    [InlineData("05", IndicadorNaturezaAcaoJudicial.AntecipacaoDeTutela)]
    [InlineData("06", IndicadorNaturezaAcaoJudicial.DepositoAdministrativoOuJudicial)]
    [InlineData("07", IndicadorNaturezaAcaoJudicial.MedidaJudicialPessoaJuridicaNaoAutora)]
    [InlineData("08", IndicadorNaturezaAcaoJudicial.SumulaVinculante)]
    [InlineData("09", IndicadorNaturezaAcaoJudicial.LiminarMandadoSegurancaColetivo)]
    [InlineData("12", IndicadorNaturezaAcaoJudicial.DecisaoNaoTransitadaExigibilidadeSuspensa)]
    [InlineData("13", IndicadorNaturezaAcaoJudicial.LiminarMandadoSegurancaExigibilidadeSuspensa)]
    [InlineData("14", IndicadorNaturezaAcaoJudicial.LiminarMedidaCautelarExigibilidadeSuspensa)]
    [InlineData("15", IndicadorNaturezaAcaoJudicial.AntecipacaoTutelaExigibilidadeSuspensa)]
    [InlineData("16", IndicadorNaturezaAcaoJudicial.DepositoExigibilidadeSuspensa)]
    [InlineData("17", IndicadorNaturezaAcaoJudicial.MedidaJudicialNaoAutoraExigibilidadeSuspensa)]
    [InlineData("19", IndicadorNaturezaAcaoJudicial.LiminarMandadoSegurancaColetivoExigibilidadeSuspensa)]
    [InlineData("99", IndicadorNaturezaAcaoJudicial.Outros)]
    public void Definidor_IndNatAcao_AtribuiEnumCorreto(string codigo, IndicadorNaturezaAcaoJudicial esperado)
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, codigo.AsSpan());

        registro.IndNatAcao.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorNaturezaAcaoJudicial.DecisaoTransitadaEmJulgadoFavoravel, "01")]
    [InlineData(IndicadorNaturezaAcaoJudicial.DecisaoNaoTransitadaEmJulgadoFavoravel, "02")]
    [InlineData(IndicadorNaturezaAcaoJudicial.LiminarMandadoSegurancaColetivo, "09")]
    [InlineData(IndicadorNaturezaAcaoJudicial.DecisaoNaoTransitadaExigibilidadeSuspensa, "12")]
    [InlineData(IndicadorNaturezaAcaoJudicial.LiminarMandadoSegurancaColetivoExigibilidadeSuspensa, "19")]
    [InlineData(IndicadorNaturezaAcaoJudicial.Outros, "99")]
    public void Serializar_IndNatAcao_RetornaCodigoSpedCorreto(IndicadorNaturezaAcaoJudicial natureza, string esperado)
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();
        registro.IndNatAcao = natureza;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1010|0001234-56.2020.1.00|4JF|10|02|Valores com exigibilidade suspensa|20032019|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1010|0001234-56.2020.1.00|4JF|10|12|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
