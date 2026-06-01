using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C040 — Identificação da ECD Recuperada. Nível hierárquico 2, ocorrência única por
/// arquivo. Identifica as informações do arquivo de ECD que foi recuperado para o período anterior.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 89–92.
/// </summary>
/// <remarks>
/// O Bloco C não é importado pelo usuário — é preenchido pelo próprio PGE do Sped Contábil após
/// a recuperação da ECD anterior via menu Escrituração/Recuperar ECD anterior.
/// </remarks>
[RegistroSped(Codigo = "C040", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC040 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C040";

    /// <summary>Hash da ECD recuperada.</summary>
    [CampoSped(Ordem = 2, Tamanho = 40, Obrigatorio = true)]
    public string? HashEcdRec { get; set; }

    /// <summary>Data inicial das informações contidas na ECD recuperada (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIniEcdRec { get; set; }

    /// <summary>Data final das informações contidas na ECD recuperada (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFinEcdRec { get; set; }

    /// <summary>CNPJ da ECD recuperada.</summary>
    [CampoSped(Ordem = 5, Tamanho = 14, Obrigatorio = true)]
    public Cnpj? CnpjEcdRec { get; set; }

    /// <summary>
    /// Indicador da forma de escrituração contábil da ECD recuperada: G = Livro Diário Geral;
    /// R = Livro Diário com Escrituração Resumida; B = Livro de Balancetes Diários.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public FormaEscrituracaoContabil IndEsc { get; set; }

    /// <summary>Código da versão do leiaute da ECD recuperada (e.g., "9.00").</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? CodVerLc { get; set; }

    /// <summary>Número de ordem da escrituração recuperada.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Obrigatorio = true)]
    public int NumOrd { get; set; }

    /// <summary>Natureza do livro (texto livre, máx. 80 caracteres).</summary>
    [CampoSped(Ordem = 9, Tamanho = 80, Obrigatorio = true)]
    public string? NatLivr { get; set; }

    /// <summary>
    /// Indicador de situação especial da ECD recuperada: 1 = Cisão; 2 = Fusão; 3 = Incorporação;
    /// 4 = Extinção. Preenchido apenas quando há evento societário.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 1)]
    public SituacaoEspecial? IndSitEspEcdRec { get; set; }

    /// <summary>
    /// Indicador de existência de NIRE na ECD recuperada: 0 = não possui; 1 = possui.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorExistenciaNire IndNireEcdRec { get; set; }

    /// <summary>
    /// Indicador da finalidade da escrituração recuperada: 0 = Original; 1 = Substituta.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFinalidadeEscrituracao IndFinEscEcdRec { get; set; }

    /// <summary>
    /// Indicador do tipo de ECD recuperada quanto à participação em SCP: 0 = não participante;
    /// 1 = participante como sócio ostensivo; 2 = ECD da SCP.
    /// </summary>
    [CampoSped(Ordem = 13, Tamanho = 1)]
    public TipoEcd? TipEcdRec { get; set; }

    /// <summary>
    /// CNPJ da SCP da ECD recuperada — preenchido apenas quando
    /// <see cref="TipEcdRec"/> é <see cref="TipoEcd.Scp"/>.
    /// </summary>
    [CampoSped(Ordem = 14, Tamanho = 14)]
    public Cnpj? CodScpEcdRec { get; set; }

    /// <summary>
    /// Identificação de moeda funcional da ECD recuperada: S = Sim; N = Não.
    /// </summary>
    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IdentMfEcdRec { get; set; }

    /// <summary>
    /// Identificação de escriturações contábeis consolidadas da ECD recuperada: S = Sim; N = Não.
    /// </summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndEscConsEcdRec { get; set; }

    /// <summary>
    /// Identificação de escrituração centralizada ou descentralizada da ECD recuperada:
    /// 0 = Centralizada; 1 = Descentralizada.
    /// </summary>
    [CampoSped(Ordem = 17, Tamanho = 1)]
    public IndicadorEscrituracaoCentralizada? IndCentralizadaEcdRec { get; set; }

    /// <summary>
    /// Indicativo de mudança de plano de contas na ECD recuperada: 0 = Não houve; 1 = Houve.
    /// </summary>
    [CampoSped(Ordem = 18, Tamanho = 1)]
    public IndicadorMudancaPlanoContas? IndMudancaPcEcdRec { get; set; }

    /// <summary>
    /// Código do Plano de Contas Referencial utilizado na ECD recuperada. Valores 0 = sem
    /// mapeamento; 1–11 conforme tabela de planos referenciais publicada pelo Sped.
    /// </summary>
    [CampoSped(Ordem = 19, Tamanho = 2)]
    public CodigoPlanoContasReferencial? IndPlanoRefEcdRec { get; set; }
}
