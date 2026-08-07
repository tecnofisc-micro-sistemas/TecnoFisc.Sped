using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests;

/// <summary>
/// Achado 3 (PR #531): sob <see cref="ReadingOptions.LenientLayout"/> o leitor emite
/// <see cref="RegistroNaoReconhecido"/> em vez de lançar — mas
/// <see cref="ArquivoEfdContribuicoes.Adicionar"/> estourava (ou, neste leiaute, silenciosamente
/// misclassificava — pois o bloco "1" existe aqui) ao tentar rotear a sentinela pelo primeiro
/// caractere do seu código cru, anulando a leitura tolerante. Cobre a coleta da sentinela e
/// confirma que registro tipado de bloco inexistente (erro de programação, não dado ruim)
/// continua lançando.
/// </summary>
public sealed class ArquivoEfdContribuicoesLenientTests
{
    // Linha |0000| real (13 campos) copiada de
    // Registro0000RoundTripTests.EscriturarOriginalSemCamposOpcionais_PreservaTextoCanonico.
    // "1999" não existe no catálogo do Bloco 1 da EFD Contribuições (ao contrário de "1010",
    // que é um registro real deste leiaute) — garante que a linha seja de fato desconhecida.
    private const string ArquivoComLinhaEstranha =
        "|0000|006|0|||01012025|31012025|EMPRESA TESTE LTDA|11222333000181|SP|3550308||00|2|\r\n" +
        "|1999|linha de bloco inexistente|\r\n" +
        "|9999|3|\r\n";

    private sealed class RegistroBlocoInexistenteSintetico : RegistroSped
    {
        public override string Codigo => "Z999";
    }

    [Fact]
    public async Task LenientLayout_ColetaBlocoDesconhecidoSemLancar()
    {
        var parser = new ParserEfdContribuicoes(new ReadingOptions { LenientLayout = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(ArquivoComLinhaEstranha));

        var arquivo = await parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        arquivo.RegistrosNaoReconhecidos.Should().ContainSingle()
            .Which.Codigo.Should().Be("1999");
    }

    [Fact]
    public void Adicionar_RegistroTipadoDeBlocoInexistente_ContinuaLancando()
    {
        var arquivo = new ArquivoEfdContribuicoes();

        var acao = () => arquivo.Adicionar(new RegistroBlocoInexistenteSintetico());

        acao.Should().Throw<InvalidOperationException>();
    }
}
