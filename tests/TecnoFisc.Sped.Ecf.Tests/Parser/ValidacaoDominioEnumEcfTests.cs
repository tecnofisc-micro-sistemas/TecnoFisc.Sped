using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Tests._Sintetico;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// Prova, via <see cref="ParserEcf"/>, que o leiaute ECF liga a validação de domínio de enum por
/// padrão e que a política chega de ponta a ponta até o leitor real.
/// </summary>
/// <remarks>
/// Ver o comentário em <c>RegistroEnumDominioSinteticoEcf</c>: nenhum registro real da ECF tem
/// hoje um campo elegível ao definidor estrito (todos os enums de <c>TecnoFisc.Sped.Ecf.Enums</c>
/// carregam <c>[SpedValor]</c>), então o teste monta seu próprio catálogo a partir de um registro
/// sintético e o injeta via <see cref="ParserEcf(IRegistroSpedCatalogo)"/> — esse construtor passa
/// pelo mesmo <see cref="ParserEcf.ResolveOptions"/> usado por <see cref="ParserEcf()"/>, então a
/// política padrão (validação ligada) testada aqui é a mesma do construtor sem catálogo.
/// <para>
/// Usa <see cref="ParserEcf.ReadStreamingAsync"/>, não <see cref="ParserEcf.ParseLinha"/>:
/// <c>ParseLinha</c> é leniente por contrato — força <c>LenientFieldParsing</c> internamente e
/// nunca lança para erro de campo (ver <c>ValidacaoDominioEnumTests</c> no Txt.Engine.Tests) —
/// então não serve para provar rejeição.
/// </para>
/// </remarks>
public sealed class ValidacaoDominioEnumEcfTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroEnumDominioSinteticoEcf).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    [Fact]
    public async Task ParserEcfPadrao_RejeitaCodigoDeEnumForaDoDominio()
    {
        var parser = new ParserEcf(_catalogo);

        var acao = async () =>
        {
            await foreach (var _ in parser.ReadStreamingAsync(
                FluxoSped("|A200|12|\r\n"), TestContext.Current.CancellationToken))
            {
            }
        };

        await acao.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task ParserEcfComValidacaoDesligada_AceitaCodigoForaDoDominio()
    {
        var parser = new ParserEcf(_catalogo, new ReadingOptions { ValidarDominioDeEnum = false });

        var registros = new List<RegistroSped>();
        await foreach (var registro in parser.ReadStreamingAsync(
            FluxoSped("|A200|12|\r\n"), TestContext.Current.CancellationToken))
            registros.Add(registro);

        var registroA200 = registros.OfType<RegistroEnumDominioSinteticoEcf>().Single();
        ((int)registroA200.TipoItem).Should().Be(12);
    }

    [Fact]
    public async Task DominioDeEnum_ForaDaFaixaDeLeiautes_ViraDiagnosticoEmVezDeExcecao()
    {
        // IND_DAD = "Z" não pertence a IndicadorMovimentoBloco.
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(13, "|0001|Z|");

        var zero0001 = registros.Single(registro => registro.Codigo == "0001");
        zero0001.ErrosDeFormato.Should().ContainSingle();
    }

    [Fact]
    public async Task DominioDeEnum_DentroDaFaixa_ContinuaSendoExcecao()
    {
        var act = async () => await LeiauteForaDaFaixaTests.ReadAsync(12, "|0001|Z|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }
}
