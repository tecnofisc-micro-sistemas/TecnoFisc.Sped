using System.Text;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Guarda de regressão de performance para <see cref="ReadingOptions.RespeitarVigenciaDoLeiaute"/>
/// (achado 9, PR #531): o gate por registro em <see cref="LeitorSpedTxt.ReadStreamingAsync"/>
/// (<c>versaoLeiaute &gt; 0 &amp;&amp; ShouldIgnoreByVersion</c>) e o <c>CampoAtivo</c> avaliado a
/// cada campo. O ponto não é provar que o caminho ligado é rápido — é provar que o caminho
/// <b>desligado</b> não paga nada, porque é o caminho hoje em produção nos três pacotes já
/// publicados no nuget.org (EFD Contribuições, EFD ICMS-IPI, ECD). Só o ECF liga vigência por
/// padrão (<c>ParserEcf.ResolverOpcoes</c>), mas aqui a flag é sempre passada explícita —
/// <c>null</c> resolveria diferente conforme o leiaute e mascararia a comparação.
/// <list type="bullet">
///   <item><see cref="SemVigencia"/> — baseline; <c>RespeitarVigenciaDoLeiaute = false</c>. O gate
///         por registro nunca avalia <c>ShouldIgnoreByVersion</c> e <c>CampoAtivo</c>
///         curto-circuita no primeiro operando (<c>!_respeitarVigencia</c>).</item>
///   <item><see cref="ComVigencia"/> — <c>true</c>. Avalia o gate a cada registro e
///         <c>CampoAtivo</c> a cada campo contra a versão declarada no <c>0000</c> (leiaute 12,
///         <c>COD_VER=0012</c>). Nenhuma linha do arquivo sintético é de fato descartada — mede o
///         custo do gate, não o efeito de descartar registros.</item>
/// </list>
/// Um <c>Ratio</c> de <see cref="SemVigencia"/> próximo de 1,00 confirma que ligar a flag no ECF
/// não regride os leiautes que a mantêm desligada.
/// </summary>
[MemoryDiagnoser]
public class ParserVigenciaBenchmark
{
    private byte[] _arquivo = null!;

    [GlobalSetup]
    public void Setup() => _arquivo = MontarArquivoEcf(registros: 10_000);

    [Benchmark(Baseline = true)]
    public async Task<int> SemVigencia()
        => await ContarAsync(new ReadingOptions { RespeitarVigenciaDoLeiaute = false });

    [Benchmark]
    public async Task<int> ComVigencia()
        => await ContarAsync(new ReadingOptions { RespeitarVigenciaDoLeiaute = true });

    private async Task<int> ContarAsync(ReadingOptions opcoes)
    {
        var parser = new ParserEcf(opcoes);
        using var stream = new MemoryStream(_arquivo, writable: false);
        int n = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            n++;
        return n;
    }

    /// <summary>
    /// Monta em memória um arquivo ECF sintético (leiaute 12, <c>COD_VER=0012</c>) com
    /// <paramref name="registros"/> registros M300 de caminho feliz. Todos os campos presentes
    /// convertem sem erro e vigoram desde a abertura do leiaute (nenhum <c>IntroduzidoEm</c>/
    /// <c>DesdeVersao</c> acima de 12), então nenhuma linha é de fato descartada por vigência —
    /// o benchmark isola o custo do gate em si.
    /// </summary>
    private static byte[] MontarArquivoEcf(int registros)
    {
        // Bloco M: M001(1) + M300(N) + M990(1) = N + 2
        // Arquivo: 0000(1) + M001(1) + M300(N) + M990(1) + 9999(1) = N + 4
        int qtdBlocoM = registros + 2;
        int totalLinhas = registros + 4;

        var sb = new StringBuilder(capacity: registros * 60 + 256);
        sb.Append("|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n");
        sb.Append("|M001|0|\r\n");
        for (int i = 1; i <= registros; i++)
        {
            // M300: CODIGO, DESCRICAO, TIPO_LANCAMENTO=A, IND_RELACAO=4, VALOR, HIST_LAN_LAL.
            // Todos os campos convertem sem erro; enums usam [SpedValor] (não afetados por
            // ValidarDominioDeEnum, que não é o alvo deste benchmark).
            sb.Append("|M300|");
            sb.Append(i);
            sb.Append("|HISTORICO DE LANCAMENTO|A|4|");
            sb.Append(i);
            sb.Append(".00|OBSERVACAO DO LANCAMENTO|\r\n");
        }
        sb.Append('|').Append("M990").Append('|').Append(qtdBlocoM).Append("|\r\n");
        sb.Append('|').Append("9999").Append('|').Append(totalLinhas).Append("|\r\n");

        return EncodingSped.Latin1.GetBytes(sb.ToString());
    }
}
