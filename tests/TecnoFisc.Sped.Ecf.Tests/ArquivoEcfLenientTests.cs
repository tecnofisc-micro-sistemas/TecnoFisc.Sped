using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests;

/// <summary>
/// Achado 3 (PR #531): <see cref="RegistroNaoReconhecido"/> tem duas origens independentes —
/// código desconhecido pelo catálogo sob <see cref="ReadingOptions.LenientLayout"/>, e descarte
/// por vigência sob <see cref="ReadingOptions.RespeitarVigenciaDoLeiaute"/> (que no ECF é
/// <c>true</c> por padrão, ver <see cref="ParserEcf.ResolverOpcoes"/> — não é opt-in). Em ambos
/// os casos <see cref="ArquivoEcf.Adicionar"/> estourava (ou, pior, misclassificava
/// silenciosamente quando o primeiro caractere do código cru coincidia com um bloco real) ao
/// tentar rotear a sentinela pelo primeiro caractere do seu código cru, anulando a leitura
/// tolerante. Cobre a coleta da sentinela nas duas origens e confirma que registro tipado de
/// bloco inexistente (erro de programação, não dado ruim) continua lançando.
/// </summary>
public sealed class ArquivoEcfLenientTests
{
    // Linha |0000| real (15 campos) copiada de Registro0000Tests.LinhaCompleta; a linha |1010|
    // não existe no catálogo ECF e nem começa com um caractere de bloco conhecido (não há
    // bloco "1"), então prova o roteamento por tipo, não por sorte de bloco.
    private const string ArquivoComLinhaEstranha =
        "|0000|LECF|0011|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
        "|1010|linha de bloco inexistente|\r\n" +
        "|9999|3|\r\n";

    // Y750 só é introduzido no leiaute 9 (ver CompatibilidadeLayoutEcfTests.IntroducoesEsperadas).
    // COD_VER=0008 no 0000 faz o leitor descartar a linha Y750 por vigência — origem
    // independente de LenientLayout, e ligada por padrão no ECF.
    private const string ArquivoComRegistroFuturo =
        "|0000|LECF|0008|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
        "|Y750|000001|DESCRICAO|VALOR|\r\n" +
        "|9999|3|\r\n";

    private sealed class RegistroBlocoInexistenteSintetico : RegistroSped
    {
        public override string Codigo => "Z999";
    }

    [Fact]
    public async Task LenientLayout_ColetaBlocoDesconhecidoSemLancar()
    {
        var parser = new ParserEcf(new ReadingOptions { LenientLayout = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(ArquivoComLinhaEstranha));

        var arquivo = await parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        arquivo.RegistrosNaoReconhecidos.Should().ContainSingle()
            .Which.Codigo.Should().Be("1010");
    }

    [Fact]
    public async Task LeituraPadrao_SemOpcoesDeclaradas_ColetaSentinelaDeVigenciaSemMisclassificarNoBlocoReal()
    {
        // new ParserEcf() sem opções: RespeitarVigenciaDoLeiaute já resolve para true por
        // padrão no ECF, então este é o caminho hoje alcançado por qualquer chamador que não
        // se pronuncia sobre opções — não é um cenário opt-in como LenientLayout.
        var parser = new ParserEcf();
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(ArquivoComRegistroFuturo));

        var arquivo = await parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        arquivo.RegistrosNaoReconhecidos.Should().ContainSingle()
            .Which.Codigo.Should().Be("Y750");
        arquivo.BlocoY.Registros.Should().BeEmpty("a sentinela não deve ser misclassificada como registro real do bloco Y");
    }

    [Fact]
    public void Adicionar_RegistroTipadoDeBlocoInexistente_ContinuaLancando()
    {
        var arquivo = new ArquivoEcf();

        var acao = () => arquivo.Adicionar(new RegistroBlocoInexistenteSintetico());

        acao.Should().Throw<InvalidOperationException>();
    }
}
