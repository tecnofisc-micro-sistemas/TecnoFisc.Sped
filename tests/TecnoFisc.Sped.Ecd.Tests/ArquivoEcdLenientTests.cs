using TecnoFisc.Sped.Ecd.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecd.Tests;

/// <summary>
/// Achado 3 (PR #531): sob <see cref="ReadingOptions.LenientLayout"/> o leitor emite
/// <see cref="RegistroNaoReconhecido"/> em vez de lançar — mas <see cref="ArquivoSpedBase{TBloco}.Adicionar"/>
/// estourava ao tentar rotear a sentinela pelo primeiro caractere do seu código cru,
/// anulando a leitura tolerante. Cobre a coleta da sentinela e confirma que registro tipado
/// de bloco inexistente (erro de programação, não dado ruim) continua lançando.
/// </summary>
public sealed class ArquivoEcdLenientTests
{
    // Linha |0000| real (22 campos, opcionais vazios) copiada de Registro0000Tests
    // (RoundTrip_ComOpcionaisVazios_PreservaTextoCanonico); a linha |1010| não existe no
    // catálogo ECD e nem começa com um caractere de bloco conhecido (não há bloco "1"),
    // então prova o roteamento por tipo, não por sorte de bloco.
    private const string ArquivoComLinhaEstranha =
        "|0000|LECD|01012023|31122023|JOAO SILVA ME|11222333000181|MG|||||0|1|0||0|0||N|N|1|1||\r\n" +
        "|1010|linha de bloco inexistente|\r\n" +
        "|9999|3|\r\n";

    private sealed class RegistroBlocoInexistenteSintetico : RegistroSped
    {
        public override string Codigo => "Z999";
    }

    [Fact]
    public async Task LenientLayout_ColetaBlocoDesconhecidoSemLancar()
    {
        var parser = new ParserEcd(new ReadingOptions { LenientLayout = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(ArquivoComLinhaEstranha));

        var arquivo = await parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        arquivo.RegistrosNaoReconhecidos.Should().ContainSingle()
            .Which.Codigo.Should().Be("1010");
    }

    [Fact]
    public void Adicionar_RegistroTipadoDeBlocoInexistente_ContinuaLancando()
    {
        var arquivo = new ArquivoEcd();

        var acao = () => arquivo.Adicionar(new RegistroBlocoInexistenteSintetico());

        acao.Should().Throw<InvalidOperationException>();
    }
}
