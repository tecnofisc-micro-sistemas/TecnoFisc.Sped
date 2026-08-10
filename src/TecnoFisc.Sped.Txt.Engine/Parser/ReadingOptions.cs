namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Opções de leitura aplicadas pelo <see cref="LeitorSpedTxt"/> (e pelos parsers de cada formato).
/// Permite ao consumidor descartar registros indesejados <b>antes</b> da materialização — útil para
/// pular registros pesados como J800/J801 da ECD (campo-arquivo RTF de até 30 MB), evitando o custo
/// de decodificar e alocar o conteúdo.
/// </summary>
/// <remarks>
/// O descarte acontece em nível de byte no leitor: registros ignorados não são decodificados nem
/// devolvidos no stream e não entram na hierarquia Pai/Filhos. Como consequência, contagens do
/// <c>9900</c>/<c>9990</c> e validações de hash do consumidor podem não fechar — é escolha de quem lê.
/// </remarks>
public sealed class ReadingOptions
{
    /// <summary>Instância padrão sem nenhum filtro (lê tudo).</summary>
    public static ReadingOptions Default { get; } = new();

    /// <summary>
    /// Códigos de registro a ignorar (ex.: <c>"J800"</c>, <c>"J801"</c>). Um registro ignorado é
    /// descartado <b>com toda a sua subárvore</b>: registros seguintes de nível hierárquico maior
    /// (filhos/netos) também são descartados, até o próximo registro de nível menor ou igual.
    /// </summary>
    public IReadOnlySet<string> RegistrosIgnorados { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Identificadores de bloco a ignorar (ex.: <c>"J"</c>, <c>"K"</c>). Todo registro cujo bloco
    /// esteja neste conjunto é descartado — incluindo abertura, detalhe e encerramento do bloco.
    /// </summary>
    public IReadOnlySet<string> BlocosIgnorados { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Quando <c>true</c>, uma falha de conversão de campo (FormatException/ArgumentException/
    /// OverflowException no Definidor) NÃO aborta a leitura: o campo fica no default, o erro é
    /// acumulado em <see cref="Abstracoes.RegistroSped.ErrosDeFormato"/> e o parsing continua.
    /// Padrão: <c>false</c> (lança ErroFormatoSpedException no primeiro erro de campo).
    /// Não afeta erros de layout (registro desconhecido) — ver <see cref="LenientLayout"/>.
    /// <para>
    /// Catraca: sob um arquivo cujo leiaute está fora da faixa conhecida pelo módulo (ver
    /// <see cref="Abstracoes.RegistroSped.IsLeiauteConhecido"/>), o leitor força este modo para
    /// <c>true</c> independentemente do valor configurado aqui — um leiaute que a biblioteca
    /// não modela não permite afirmar que o dado está errado. Não há como pedir fail-fast nesse
    /// cenário; passar <c>false</c> explicitamente não tem efeito enquanto o leiaute for
    /// desconhecido.
    /// </para>
    /// </summary>
    public bool LenientFieldParsing { get; init; }

    /// <summary>
    /// Quando <c>true</c>, um código de registro desconhecido pelo catálogo NÃO aborta a leitura:
    /// o leitor emite um <see cref="Abstracoes.RegistroNaoReconhecido"/> (linha crua + erro) como
    /// folha na hierarquia e continua. Padrão: <c>false</c> (lança ErroLayoutSpedException,
    /// comportamento atual). Independente de <see cref="LenientFieldParsing"/>.
    /// <para>
    /// Catraca: mesma ressalva de <see cref="LenientFieldParsing"/> — sob leiaute fora da faixa
    /// conhecida, o leitor força este modo para <c>true</c> independentemente do valor
    /// configurado aqui, pelo mesmo motivo.
    /// </para>
    /// </summary>
    public bool LenientLayout { get; init; }

    /// <summary>
    /// Quando <c>true</c>, omite registros anteriores a <c>IntroduzidoEm</c> e não atribui
    /// campos anteriores a <c>DesdeVersao</c>, usando a versão declarada pelo registro 0000.
    /// <c>null</c> (padrão) delega a decisão ao parser do leiaute: o ECF liga, os demais
    /// leiautes read-only mantêm o modelo informacional completo e não ligam.
    /// </summary>
    public bool? RespeitarVigenciaDoLeiaute { get; init; }

    /// <summary>
    /// Quando <c>true</c>, um código numérico fora do domínio declarado de um enum fechado
    /// (sem <c>[SpedValor]</c>) vira erro de campo em vez de cast permissivo. <c>null</c>
    /// (padrão) delega a decisão ao parser do leiaute: o ECF liga, os demais mantêm o cast
    /// permissivo — a Receita publica códigos novos entre versões do guia e um arquivo que
    /// hoje é lido não pode passar a falhar por atualização de pacote.
    /// <para>
    /// Ao contrário de <see cref="LenientFieldParsing"/> e <see cref="LenientLayout"/>, esta
    /// opção <b>não</b> é forçada pelo gate de leiaute fora da faixa conhecida — a validação de
    /// domínio permanece exatamente como configurada aqui em qualquer leiaute. Desligá-la
    /// faria um valor fora do domínio ser aceito em silêncio (cast permissivo, sem exceção,
    /// sem diagnóstico); quem converte a exceção de domínio em diagnóstico sob leiaute
    /// desconhecido é o alargamento de <see cref="LenientFieldParsing"/>, não o desligamento
    /// desta validação.
    /// </para>
    /// </summary>
    public bool? ValidarDominioDeEnum { get; init; }

    /// <summary>
    /// <c>true</c> quando há ao menos um filtro configurado. O leitor usa isto para pular toda a
    /// lógica de descarte (fast-path) quando nada deve ser ignorado.
    /// </summary>
    internal bool HasFilter => RegistrosIgnorados.Count > 0 || BlocosIgnorados.Count > 0;
}
