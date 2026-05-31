using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J932 — Signatários do Termo de Verificação para Fins de Substituição da ECD.
/// Nível hierárquico 3, ocorrência 0:2 por arquivo.
/// Obrigatório quando a ECD for substituta (<c>IND_FIN_ESC</c> = "1" no Registro 0000).
/// Identifica os signatários do Termo de Verificação: o contador/contabilista responsável
/// (<see cref="CodigoQualificacaoAssinante.ContadorTermoSubstituicao"/>, código 910) e,
/// quando a ECD tiver sido auditada por auditor independente, também o auditor
/// (<see cref="CodigoQualificacaoAssinante.AuditorIndependenteTermoSubstituicao"/>, código 920).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 203–206.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OBRIGATORIO_CONTADOR_ASS_TERMO</c>: deve existir ao menos um J932 com
///   <see cref="CodAssinT"/> = 910 (Contador/Contabilista Responsável pelo Termo).</item>
///   <item><c>REGRA_IDENT_CPF_CNPJ_COD_ASSIN_DUPLICIDADE</c>: a chave
///   <see cref="IdentCpfCnpjT"/> + <see cref="CodAssinT"/> não pode ser duplicada.</item>
///   <item><c>REGRA_OBRIGATORIO_ASS_TERMO</c>: quando <see cref="CodAssinT"/> = 910, os campos
///   <see cref="IndCrcT"/>, <see cref="EmailT"/>, <see cref="FoneT"/> e <see cref="UfCrcT"/>
///   são obrigatórios.</item>
///   <item><c>REGRA_QUALIF_INVALIDA_ASS_TERMO</c>: quando <see cref="IdentCpfCnpjT"/> for CNPJ,
///   <see cref="CodAssinT"/> deve ser 920 (Auditor Independente).</item>
///   <item><c>REGRA_TABELA_ASSINANTE_DESC</c>: quando <see cref="CodAssinT"/> = 910, verifica
///   se <see cref="IdentQualifT"/> corresponde ao Contador/Contabilista Responsável.</item>
///   <item><c>REGRA_VALIDA_CPF</c> / <c>REGRA_VALIDA_CNPJ</c>: valida o dígito verificador de
///   <see cref="IdentCpfCnpjT"/> conforme o tamanho (11 = CPF, 14 = CNPJ).</item>
///   <item><c>REGRA_TABELA_UF</c>: <see cref="UfCrcT"/> deve existir na Tabela de UFs.</item>
///   <item><c>REGRA_VALIDA_FORMATO_SEQUENCIAL_CRC</c>: <see cref="NumSeqCrcT"/> no formato
///   UF/YYYY/NÚMERO.</item>
///   <item><c>REGRA_ADV_ASS_CONTADOR_TERMO</c>: quando <see cref="CodAssinT"/> = 910, gera aviso
///   se <see cref="NumSeqCrcT"/> ou <see cref="DtCrcT"/> não estiverem preenchidos.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J932", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ932 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J932";

    /// <summary>
    /// Nome do signatário do termo de verificação.
    /// Campo <c>IDENT_NOM_T</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? IdentNomT { get; set; }

    /// <summary>
    /// CPF (11 dígitos) ou CNPJ (14 dígitos) do assinante do termo de verificação.
    /// Campo <c>IDENT_CPF_CNPJ_T</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? IdentCpfCnpjT { get; set; }

    /// <summary>
    /// Qualificação do assinante do termo de verificação conforme tabela (descrição textual).
    /// Campo <c>IDENT_QUALIF_T</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? IdentQualifT { get; set; }

    /// <summary>
    /// Código de qualificação do assinante do termo de verificação.
    /// Valores válidos: 910 (Contador/Contabilista) ou 920 (Auditor Independente).
    /// Campo <c>COD_ASSIN_T</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public CodigoQualificacaoAssinante CodAssinT { get; set; }

    /// <summary>
    /// Número de inscrição do contabilista no Conselho Regional de Contabilidade.
    /// Obrigatório quando <see cref="CodAssinT"/> = 910 (Contador/Contabilista).
    /// Campo <c>IND_CRC_T</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? IndCrcT { get; set; }

    /// <summary>
    /// E-mail do signatário do termo de verificação.
    /// Obrigatório quando <see cref="CodAssinT"/> = 910 (Contador/Contabilista).
    /// Campo <c>EMAIL_T</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 60)]
    public string? EmailT { get; set; }

    /// <summary>
    /// Telefone do signatário do termo de verificação.
    /// Obrigatório quando <see cref="CodAssinT"/> = 910 (Contador/Contabilista).
    /// Campo <c>FONE_T</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 14)]
    public string? FoneT { get; set; }

    /// <summary>
    /// Sigla da Unidade da Federação que expediu o CRC (2 caracteres).
    /// Obrigatório quando <see cref="CodAssinT"/> = 910 (Contador/Contabilista).
    /// Campo <c>UF_CRC_T</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? UfCrcT { get; set; }

    /// <summary>
    /// Número da Certidão de Regularidade Profissional do Contador no formato UF/YYYY/NÚMERO.
    /// Campo <c>NUM_SEQ_CRC_T</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? NumSeqCrcT { get; set; }

    /// <summary>
    /// Data de validade da Certidão de Regularidade Profissional do Contador (ddMMyyyy).
    /// Campo <c>DT_CRC_T</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtCrcT { get; set; }
}
