using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecd;

/// <summary>
/// Modelo raiz do arquivo ECD (Escrituração Contábil Digital — leiaute 9). Agrupa os
/// registros nos seis blocos do leiaute na ordem canônica (0, C, I, J, K, 9).
/// </summary>
/// <remarks>
/// Pacote read-only (ARCHITECTURE §2.5): os registros vêm vinculados via
/// <c>PilhaHierarquica</c> a partir do <see cref="Parser.ParserEcd"/>; o arquivo apenas os
/// redistribui em blocos preservando a ordem original. Não existe gerador associado — a ECD
/// é exclusivamente leitura.
/// </remarks>
public sealed class ArquivoEcd : ArquivoSpedBase<BlocoEcd>
{
    private static readonly string[] _ordemBlocos =
        ["0", "C", "I", "J", "K", "9"];

    public ArquivoEcd() : base(_ordemBlocos, id => new BlocoEcd(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "ECD";

    /// <inheritdoc />
    protected override void AdicionarAoBloco(BlocoEcd bloco, RegistroSped registro)
        => bloco.Adicionar(registro);

    public BlocoEcd Bloco0 => Bloco("0");
    public BlocoEcd BlocoC => Bloco("C");
    public BlocoEcd BlocoI => Bloco("I");
    public BlocoEcd BlocoJ => Bloco("J");
    public BlocoEcd BlocoK => Bloco("K");
    public BlocoEcd Bloco9 => Bloco("9");

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco.
    /// </summary>
    public static async Task<ArquivoEcd> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEcd();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
