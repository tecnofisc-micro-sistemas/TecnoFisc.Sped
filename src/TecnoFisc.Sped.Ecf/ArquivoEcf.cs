using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf;

/// <summary>
/// Modelo raiz read-only de um arquivo ECF. Agrupa os registros nos 17 blocos do leiaute
/// em sua ordem canônica.
/// </summary>
public sealed class ArquivoEcf : ArquivoSpedBase<BlocoEcf>
{
    private static readonly string[] _ordemBlocos = ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    public ArquivoEcf() : base(_ordemBlocos, id => new BlocoEcf(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "ECF";

    /// <inheritdoc />
    protected override void AdicionarAoBloco(BlocoEcf bloco, RegistroSped registro)
        => bloco.Adicionar(registro);

    public BlocoEcf Bloco0 => Bloco("0");
    public BlocoEcf BlocoC => Bloco("C");
    public BlocoEcf BlocoE => Bloco("E");
    public BlocoEcf BlocoJ => Bloco("J");
    public BlocoEcf BlocoK => Bloco("K");
    public BlocoEcf BlocoL => Bloco("L");
    public BlocoEcf BlocoM => Bloco("M");
    public BlocoEcf BlocoN => Bloco("N");
    public BlocoEcf BlocoP => Bloco("P");
    public BlocoEcf BlocoQ => Bloco("Q");
    public BlocoEcf BlocoT => Bloco("T");
    public BlocoEcf BlocoU => Bloco("U");
    public BlocoEcf BlocoV => Bloco("V");
    public BlocoEcf BlocoW => Bloco("W");
    public BlocoEcf BlocoX => Bloco("X");
    public BlocoEcf BlocoY => Bloco("Y");
    public BlocoEcf Bloco9 => Bloco("9");

    /// <summary>Constrói o arquivo a partir do fluxo do parser.</summary>
    public static async Task<ArquivoEcf> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEcf();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
