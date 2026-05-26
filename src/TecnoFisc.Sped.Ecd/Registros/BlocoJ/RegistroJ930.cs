using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J930 — Signatários da Escrituração. Nível hierárquico 3, ocorrência 1:N por arquivo.
/// Identifica cada signatário da ECD. Toda ECD deve ter ao menos um signatário com
/// <see cref="CodAssin"/> igual a <see cref="CodigoQualificacaoAssinante.ContadorContabilista"/> (900)
/// e exatamente um signatário com <see cref="IndRespLegal"/> igual a
/// <see cref="IndicadorSimNao.Sim"/>.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 197–203.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OBRIGATORIO_ASSIN_CONTADOR</c>: deve existir ao menos um J930 com
///   <see cref="CodAssin"/> = 900 (Contador/Contabilista).</item>
///   <item><c>REGRA_IDENT_CPF_CNPJ_COD_ASSIN_DUPLICIDADE</c>: a chave
///   <see cref="IdentCpfCnpj"/> + <see cref="CodAssin"/> não pode ser duplicada.</item>
///   <item><c>REGRA_OBRIGATORIO_UM_RESP_LEGAL</c>: deve existir exatamente um J930 com
///   <see cref="IndRespLegal"/> = "S"; o código 900 (Contador/Contabilista) não pode ser
///   o responsável legal.</item>
///   <item><c>REGRA_OBRIGATORIO_CONTADOR</c>: quando <see cref="CodAssin"/> = 900, os campos
///   <see cref="IndCrc"/>, <see cref="Email"/>, <see cref="Fone"/> e <see cref="UfCrc"/>
///   tornam-se obrigatórios.</item>
///   <item><c>REGRA_AVISO_ASSIN_CNPJ</c>: quando <see cref="CodAssin"/> = "001" (e-CNPJ/e-PJ),
///   os campos <see cref="IndCrc"/>, <see cref="UfCrc"/>, <see cref="NumSeqCrc"/> e
///   <see cref="DtCrc"/> geram aviso se não preenchidos.</item>
///   <item><c>REGRA_VALIDA_CPF</c> / <c>REGRA_VALIDA_CNPJ</c>: valida o dígito verificador de
///   <see cref="IdentCpfCnpj"/> conforme o tamanho (11 = CPF, 14 = CNPJ).</item>
///   <item><c>REGRA_TABELA_ASSINANTE_DESC</c>: quando <see cref="CodAssin"/> = 900, verifica
///   se <see cref="IdentQualif"/> corresponde a Contador/Contabilista.</item>
///   <item><c>REGRA_QUALIF_INV_RESP_LEGAL</c>: <see cref="CodAssin"/> diferente de 900 quando
///   <see cref="IndRespLegal"/> = "S".</item>
///   <item><c>REGRA_TABELA_UF</c>: <see cref="UfCrc"/> deve existir na Tabela de Unidades da
///   Federação.</item>
///   <item><c>REGRA_VALIDA_FORMATO_SEQUENCIAL_CRC</c>: <see cref="NumSeqCrc"/> no formato
///   UF/YYYY/NÚMERO.</item>
///   <item><c>REGRA_ADVERTENCIA_CONTADOR</c>: quando <see cref="CodAssin"/> = 900, gera aviso
///   se <see cref="NumSeqCrc"/> ou <see cref="DtCrc"/> não estiverem preenchidos.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J930", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ930 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J930";

    /// <summary>
    /// Nome do signatário.
    /// Campo <c>IDENT_NOM</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? IdentNom { get; set; }

    /// <summary>
    /// CPF (11 dígitos) ou CNPJ (14 dígitos) do signatário.
    /// Campo <c>IDENT_CPF_CNPJ</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? IdentCpfCnpj { get; set; }

    /// <summary>
    /// Qualificação do assinante conforme a Tabela de Qualificação do Assinante (descrição textual).
    /// Campo <c>IDENT_QUALIF</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? IdentQualif { get; set; }

    /// <summary>
    /// Código de qualificação do assinante conforme a Tabela de Qualificação do Assinante.
    /// Campo <c>COD_ASSIN</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public CodigoQualificacaoAssinante CodAssin { get; set; }

    /// <summary>
    /// Número de inscrição do contabilista no Conselho Regional de Contabilidade.
    /// Obrigatório quando <see cref="CodAssin"/> = 900 (Contador/Contabilista).
    /// Campo <c>IND_CRC</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? IndCrc { get; set; }

    /// <summary>
    /// E-mail do signatário.
    /// Obrigatório quando <see cref="CodAssin"/> = 900 (Contador/Contabilista).
    /// Campo <c>EMAIL</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 60)]
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do signatário.
    /// Obrigatório quando <see cref="CodAssin"/> = 900 (Contador/Contabilista).
    /// Campo <c>FONE</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 14)]
    public string? Fone { get; set; }

    /// <summary>
    /// Sigla da Unidade da Federação que expediu o CRC (2 caracteres).
    /// Obrigatório quando <see cref="CodAssin"/> = 900 (Contador/Contabilista).
    /// Campo <c>UF_CRC</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? UfCrc { get; set; }

    /// <summary>
    /// Número da Certidão de Regularidade Profissional do Contador no formato UF/YYYY/NÚMERO.
    /// Campo <c>NUM_SEQ_CRC</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? NumSeqCrc { get; set; }

    /// <summary>
    /// Data de validade da Certidão de Regularidade Profissional do Contador (ddMMyyyy).
    /// Campo <c>DT_CRC</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtCrc { get; set; }

    /// <summary>
    /// Indica se este signatário é o responsável pela assinatura da ECD que será validado
    /// perante a RFB (S = Sim, N = Não). Apenas um J930 por arquivo pode ter "S".
    /// O código 900 (Contador/Contabilista) não pode ser o responsável legal.
    /// Campo <c>IND_RESP_LEGAL</c>.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRespLegal { get; set; }
}
