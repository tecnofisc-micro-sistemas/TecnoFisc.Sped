using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdIcmsIpi;

/// <summary>
/// Modelo raiz do arquivo EFD ICMS-IPI (baseline V015). Agrupa os registros nos dez
/// blocos do leiaute na ordem canônica (0, B, C, D, E, G, H, K, 1, 9).
/// </summary>
/// <remarks>
/// Pacote read-only (ARCHITECTURE §2.5): os registros vêm vinculados via
/// <c>PilhaHierarquica</c> a partir do <see cref="Parser.ParserEfdIcmsIpi"/>; o arquivo apenas
/// os redistribui em blocos preservando a ordem original. Não existe gerador associado —
/// EFD ICMS-IPI é exclusivamente leitura.
/// </remarks>
public sealed class ArquivoEfdIcmsIpi : ArquivoSpedBase<BlocoEfdIcmsIpi>
{
    private static readonly string[] _ordemBlocos =
        ["0", "B", "C", "D", "E", "G", "H", "K", "1", "9"];

    public ArquivoEfdIcmsIpi() : base(_ordemBlocos, id => new BlocoEfdIcmsIpi(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "EFD ICMS-IPI";

    /// <inheritdoc />
    protected override void AdicionarAoBloco(BlocoEfdIcmsIpi bloco, RegistroSped registro)
        => bloco.Adicionar(registro);

    public BlocoEfdIcmsIpi Bloco0 => Bloco("0");
    public BlocoEfdIcmsIpi BlocoB => Bloco("B");
    public BlocoEfdIcmsIpi BlocoC => Bloco("C");
    public BlocoEfdIcmsIpi BlocoD => Bloco("D");
    public BlocoEfdIcmsIpi BlocoE => Bloco("E");
    public BlocoEfdIcmsIpi BlocoG => Bloco("G");
    public BlocoEfdIcmsIpi BlocoH => Bloco("H");
    public BlocoEfdIcmsIpi BlocoK => Bloco("K");
    public BlocoEfdIcmsIpi Bloco1 => Bloco("1");
    public BlocoEfdIcmsIpi Bloco9 => Bloco("9");

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco.
    /// </summary>
    public static async Task<ArquivoEfdIcmsIpi> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEfdIcmsIpi();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
