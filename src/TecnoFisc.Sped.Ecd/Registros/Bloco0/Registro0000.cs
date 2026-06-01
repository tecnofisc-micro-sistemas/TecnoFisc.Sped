using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecd.Enums;
using TecnoFisc.Sped.Ecd.Versionamento;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0000 — Abertura do Arquivo Digital e Identificação do Empresário ou da Sociedade
/// Empresária. Nível hierárquico 0, ocorrência única por arquivo. Conforme Manual de Orientação
/// do Leiaute 9 da ECD, p. 64.
/// </summary>
/// <remarks>
/// <para>
/// O <c>partial</c> deixa o source generator do Stage 6 estender a classe com o catálogo
/// gerado em tempo de compilação; até lá o catálogo é montado por reflexão cacheada.
/// </para>
/// <para>
/// Diferente do EFD, o <c>0000</c> da ECD não tem <c>COD_VER</c>: o campo 02 é o literal
/// <c>"LECD"</c> (<see cref="Lecd"/>). A versão do leiaute contábil é declarada em
/// <c>I010.COD_VER_LC</c>; como o pacote modela um único leiaute, <see cref="VersaoLeiaute"/>
/// reporta o leiaute 9.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0000", Nivel = 0, Bloco = "0")]
public sealed partial class Registro0000 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0000";

    /// <inheritdoc />
    public override int VersaoLeiaute => (int)LayoutEcd.V009;

    /// <summary>Texto fixo contendo o identificador do arquivo da ECD: <c>"LECD"</c>.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? Lecd { get; set; } = "LECD";

    /// <summary>Data inicial das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Nome empresarial da pessoa jurídica.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// Número de inscrição da pessoa jurídica no CNPJ. No arquivo da SCP é sempre o CNPJ da
    /// sócia ostensiva.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 14, Obrigatorio = true)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>Sigla da unidade da federação da pessoa jurídica (2 letras).</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Inscrição Estadual da pessoa jurídica — preenchida quando houver.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public string? Ie { get; set; }

    /// <summary>Código do município do domicílio fiscal conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 9, Tamanho = 7)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição Municipal da pessoa jurídica — preenchida quando houver.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? Im { get; set; }

    /// <summary>
    /// Indicador de situação especial (conforme tabela publicada pelo Sped). Preenchido apenas
    /// quando há evento societário no período.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 1)]
    public SituacaoEspecial? IndSitEsp { get; set; }

    /// <summary>Indicador de situação no início do período (conforme tabela publicada pelo Sped).</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public SituacaoInicioPeriodo IndSitIniPer { get; set; }

    /// <summary>Indicador de existência de NIRE.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorExistenciaNire IndNire { get; set; }

    /// <summary>Indicador de finalidade da escrituração: 0 = original, 1 = substituta.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFinalidadeEscrituracao IndFinEsc { get; set; }

    /// <summary>
    /// Hash da escrituração substituída — preenchido quando <see cref="IndFinEsc"/> é
    /// <see cref="IndicadorFinalidadeEscrituracao.Substituta"/>.
    /// </summary>
    [CampoSped(Ordem = 15, Tamanho = 40)]
    public string? CodHashSub { get; set; }

    /// <summary>Indicador de entidade sujeita a auditoria independente.</summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true)]
    public IndicadorGrandePorte IndGrandePorte { get; set; }

    /// <summary>Indicador do tipo de ECD quanto à participação em SCP.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public TipoEcd TipEcd { get; set; }

    /// <summary>
    /// CNPJ da SCP — preenchido apenas pela própria SCP (<see cref="TipEcd"/> =
    /// <see cref="TipoEcd.Scp"/>), nunca pelo sócio ostensivo.
    /// </summary>
    [CampoSped(Ordem = 18, Tamanho = 14)]
    public Cnpj? CodScp { get; set; }

    /// <summary>
    /// Identificação de moeda funcional: indica que a escrituração abrange valores com base na
    /// moeda funcional (S/N).
    /// </summary>
    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IdentMf { get; set; }

    /// <summary>
    /// Escriturações contábeis consolidadas (S/N) — preenchido pela empresa controladora obrigada
    /// a informar demonstrações contábeis consolidadas.
    /// </summary>
    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndEscCons { get; set; }

    /// <summary>Indicador da modalidade de escrituração centralizada ou descentralizada.</summary>
    [CampoSped(Ordem = 21, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEscrituracaoCentralizada IndCentralizada { get; set; }

    /// <summary>Indicador de mudança de plano de contas.</summary>
    [CampoSped(Ordem = 22, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMudancaPlanoContas IndMudancPc { get; set; }

    /// <summary>
    /// Código do Plano de Contas Referencial usado no mapeamento das contas analíticas. Fica em
    /// branco quando a pessoa jurídica não realiza o mapeamento.
    /// </summary>
    [CampoSped(Ordem = 23, Tamanho = 2)]
    public CodigoPlanoContasReferencial? CodPlanRef { get; set; }
}
