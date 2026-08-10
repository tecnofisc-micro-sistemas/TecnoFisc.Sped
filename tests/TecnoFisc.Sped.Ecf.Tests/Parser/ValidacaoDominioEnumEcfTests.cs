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
/// Validação de domínio de enum no ECF. Os enums elegíveis ao setter estrito não vivem em
/// <c>TecnoFisc.Sped.Ecf.Enums</c> e sim em <c>TecnoFisc.Sped.Txt.Engine.Enums</c>:
/// <c>IndicadorMovimentoBloco</c> (campo <c>IND_DAD</c> dos 19 registros de abertura de bloco) e
/// <c>CodigoNaturezaContaContabil</c> (campo <c>COD_NAT</c> de C050 e J050). Como o ECF usa
/// <c>ValidarDominioDeEnum = true</c> por padrão, esses campos são caminho de produção.
/// <para>
/// Usa <see cref="ParserEcf.ReadStreamingAsync"/>, não <c>ParserEcf.ParseLinha</c>:
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
    public async Task FalhaDeConversaoDeCampo_ForaDaFaixaDeLeiautes_ViraDiagnosticoEmVezDeExcecao()
    {
        // IND_DAD = "Z" nem chega a ser um int válido: falha no próprio int.Parse, antes de
        // qualquer checagem de domínio (setter estrito e permissivo compartilham o parse).
        // Exercita o alargamento de LenientFieldParsing (lenienteCampo), não a validação de
        // domínio — ver DominioDeEnum_* abaixo para o caso que de fato exercita domínio.
        var registros = await FixtureEcf.ReadAsync(13, "|0001|Z|");

        var zero0001 = registros.Single(registro => registro.Codigo == "0001");
        zero0001.ErrosDeFormato.Should().ContainSingle();
    }

    [Fact]
    public async Task FalhaDeConversaoDeCampo_DentroDaFaixa_ContinuaSendoExcecao()
    {
        var act = async () => await FixtureEcf.ReadAsync(12, "|0001|Z|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task DominioDeEnum_ForaDaFaixaDeLeiautes_ViraDiagnosticoEmVezDeExcecao()
    {
        // IND_DAD = "9" é um int válido, mas fora do domínio de IndicadorMovimentoBloco
        // (só define 0 e 1) — exercita de fato o setter estrito (Enum.IsDefined) e prova que a
        // validação de domínio continua ligada fora da faixa conhecida: quem converte a
        // exceção em diagnóstico é o lenienteCampo alargado, não o desligamento da validação.
        var registros = await FixtureEcf.ReadAsync(13, "|0001|9|");

        var zero0001 = registros.Single(registro => registro.Codigo == "0001");
        zero0001.ErrosDeFormato.Should().ContainSingle();
    }

    [Fact]
    public async Task DominioDeEnum_DentroDaFaixa_ContinuaSendoExcecao()
    {
        var act = async () => await FixtureEcf.ReadAsync(12, "|0001|9|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    // Nota: um teste "IndDad_ForaDoDominio_AbortaNoLeiauteConhecido" com IND_DAD = "Z" seria
    // idêntico a FalhaDeConversaoDeCampo_DentroDaFaixa_ContinuaSendoExcecao acima (mesma
    // chamada, mesma asserção) — omitido para não duplicar cobertura já existente.

    [Fact]
    public async Task IndDad_DentroDoDominio_EhLido()
    {
        var registros = await FixtureEcf.ReadAsync(12, "|0001|0|");

        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task CodNat_ForaDoDominio_AbortaNoLeiauteConhecido()
    {
        // C050 traz COD_NAT; "99" não pertence a CodigoNaturezaContaContabil.
        var act = async () => await FixtureEcf.ReadAsync(
            12, "|C001|0|\r\n|C050|01012025|99|A|1|CTA001||CONTA TESTE|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }
}
