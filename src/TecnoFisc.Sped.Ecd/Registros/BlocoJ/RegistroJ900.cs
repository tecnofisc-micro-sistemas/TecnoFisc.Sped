using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J900 — Termo de Encerramento. Nível hierárquico 2, ocorrência 1:1 por arquivo.
/// Fornece os dados do termo de encerramento da escrituração.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 195–197.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OCORRENCIA_UNITARIA_ARQ</c>: registro ocorre exatamente uma vez por arquivo.</item>
///   <item><c>REGRA_IGUAL_NUM_ORD_REGI030</c>: <see cref="NumOrd"/> deve ser igual ao <c>NUM_ORD</c> do Registro I030.</item>
///   <item><c>REGRA_VALIDA_CONTEUDO_NAT_LIVR</c>: <see cref="NatLivro"/> deve ser igual ao <c>NAT_LIVR</c> do Registro I030.</item>
///   <item><c>REGRA_IGUAL_NOME_REG0000</c>: <see cref="Nome"/> deve ser igual ao campo <c>NOME</c> do Registro 0000.</item>
///   <item><c>REGRA_IGUAL_QTD_LIN_REG9999</c>: <see cref="QtdLin"/> deve ser igual ao campo <c>QTD_LIN</c> do Registro 9999.</item>
///   <item><c>REGRA_IGUAL_DT_INI_REG0000</c>: <see cref="DtIniEscr"/> deve ser igual ao campo <c>DT_INI</c> do Registro 0000.</item>
///   <item><c>REGRA_IGUAL_DT_FIN_REG0000</c>: <see cref="DtFinEscr"/> deve ser igual ao campo <c>DT_FIN</c> do Registro 0000.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J900", Nivel = 2, Bloco = "J")]
public sealed partial class RegistroJ900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J900";

    /// <summary>
    /// Texto fixo contendo <c>"TERMO DE ENCERRAMENTO"</c> (21 caracteres).
    /// Campo <c>DNRC_ENCER</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 21, Obrigatorio = true)]
    public string? DnrcEncer { get; set; } = "TERMO DE ENCERRAMENTO";

    /// <summary>
    /// Número de ordem do instrumento de escrituração.
    /// Campo <c>NUM_ORD</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public int NumOrd { get; set; }

    /// <summary>
    /// Natureza do livro; finalidade a que se destinou o instrumento (até 80 caracteres).
    /// Campo <c>NAT_LIVRO</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 80, Obrigatorio = true)]
    public string? NatLivro { get; set; }

    /// <summary>
    /// Nome empresarial.
    /// Campo <c>NOME</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// Quantidade total de linhas do arquivo digital.
    /// Campo <c>QTD_LIN</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public int QtdLin { get; set; }

    /// <summary>
    /// Data de início da escrituração (ddMMyyyy).
    /// Campo <c>DT_INI_ESCR</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIniEscr { get; set; }

    /// <summary>
    /// Data de término da escrituração (ddMMyyyy).
    /// Campo <c>DT_FIN_ESCR</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFinEscr { get; set; }
}
