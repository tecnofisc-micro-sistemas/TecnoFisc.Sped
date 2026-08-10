using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdContribuicoes;

/// <summary>
/// Modelo raiz do arquivo EFD Contribuições (V006, guia v1.35). Agrupa os registros nos dez
/// blocos do leiaute na ordem canônica (0, A, C, D, F, I, M, P, 1, 9). Atende o contrato de
/// <see cref="IArquivoSped"/> e expõe acesso direto a cada bloco para consumidores tipados.
/// </summary>
/// <remarks>
/// Os registros já vêm vinculados via <c>PilhaHierarquica</c> quando o arquivo é montado a
/// partir do <see cref="Parser.ParserEfdContribuicoes"/>; o arquivo apenas os redistribui em
/// blocos preservando a ordem original. Para gravar, percorra
/// <see cref="ArquivoSpedBase{TBloco}.EnumerarRegistros"/> e entregue ao
/// <see cref="Gerador.GeradorEfdContribuicoes"/>.
/// </remarks>
public sealed class ArquivoEfdContribuicoes : ArquivoSpedBase<BlocoEfdContribuicoes>
{
    private static readonly string[] _ordemBlocos =
        ["0", "A", "C", "D", "F", "I", "M", "P", "1", "9"];

    public ArquivoEfdContribuicoes() : base(_ordemBlocos, id => new BlocoEfdContribuicoes(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "EFD Contribuições";

    /// <inheritdoc />
    protected override void AdicionarAoBloco(BlocoEfdContribuicoes bloco, RegistroSped registro)
        => bloco.Adicionar(registro);

    public BlocoEfdContribuicoes Bloco0 => Bloco("0");
    public BlocoEfdContribuicoes BlocoA => Bloco("A");
    public BlocoEfdContribuicoes BlocoC => Bloco("C");
    public BlocoEfdContribuicoes BlocoD => Bloco("D");
    public BlocoEfdContribuicoes BlocoF => Bloco("F");
    public BlocoEfdContribuicoes BlocoI => Bloco("I");
    public BlocoEfdContribuicoes BlocoM => Bloco("M");
    public BlocoEfdContribuicoes BlocoP => Bloco("P");
    public BlocoEfdContribuicoes Bloco1 => Bloco("1");
    public BlocoEfdContribuicoes Bloco9 => Bloco("9");

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco. Útil como ponte direta entre <see cref="Parser.ParserEfdContribuicoes"/>
    /// e a API tipada.
    /// </summary>
    public static async Task<ArquivoEfdContribuicoes> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEfdContribuicoes();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
