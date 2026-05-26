using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J935 — Identificação dos Auditores Independentes.
/// Nível hierárquico 3, ocorrência 0:N por arquivo.
/// Obrigatório quando o campo <c>IND_GRANDE_PORTE</c> (Campo 16) do Registro 0000 for igual a "1"
/// (empresa sujeita a auditoria independente — Ativo Total superior a R$ 240.000.000,00 ou
/// Receita Bruta Anual superior a R$ 300.000.000,00).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 207.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_VALIDA_CPF</c>: verifica a regra de formação do CPF informado em
///   <see cref="NiCpfCnpj"/>.</item>
///   <item><c>REGRA_VALIDA_CNPJ</c>: verifica a regra de formação do CNPJ informado em
///   <see cref="NiCpfCnpj"/>.</item>
///   <item><c>REGRA_COD_CVM_AUD_OBRIGATORIO</c>: quando <see cref="NiCpfCnpj"/> for CPF
///   (pessoa física), o campo <see cref="CodCvmAuditor"/> deve ser informado.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J935", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ935 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J935";

    /// <summary>
    /// CPF (11 dígitos) ou CNPJ (14 dígitos) do auditor independente ou da pessoa jurídica de
    /// auditoria independente.
    /// Campo <c>NI_CPF_CNPJ</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? NiCpfCnpj { get; set; }

    /// <summary>
    /// Nome do auditor independente ou da pessoa jurídica de auditoria independente.
    /// Campo <c>NOME_AUDITOR_FIRMA</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? NomeAuditorFirma { get; set; }

    /// <summary>
    /// Registro do auditor independente na CVM (Comissão de Valores Mobiliários).
    /// Obrigatório quando <see cref="NiCpfCnpj"/> for CPF (auditor pessoa física).
    /// Campo <c>COD_CVM_AUDITOR</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? CodCvmAuditor { get; set; }
}
