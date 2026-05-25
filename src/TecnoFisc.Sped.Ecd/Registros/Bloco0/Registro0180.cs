using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0180 — Identificação do Relacionamento com o Participante. Nível hierárquico 3,
/// ocorrência 0:N. Informa os códigos de relacionamento dos participantes listados no
/// <see cref="Registro0150"/>, a data de início e a data de término do relacionamento, caso exista.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 86.
/// </summary>
/// <remarks>
/// <para>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_TABELA_RELACIONAMENTO</c>: COD_REL (campo 02) deve estar na Tabela de
///   Códigos de Participação do Participante publicada pelo Sped.</item>
///   <item><c>REGRA_DT_INI_MAIOR_DT_FIN_REL</c>: DT_INI_REL (campo 03) deve ser menor ou
///   igual a DT_FIN_REL (campo 04) quando preenchido — gera aviso, não erro.</item>
/// </list>
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0180", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0180 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0180";

    /// <summary>
    /// Código do relacionamento do participante conforme tabela publicada pelo Sped.
    /// Campo <c>COD_REL</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoRelacionamentoParticipante CodRel { get; set; }

    /// <summary>Data do início do relacionamento (ddMMyyyy). Campo <c>DT_INI_REL</c>.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIniRel { get; set; }

    /// <summary>Data do término do relacionamento (ddMMyyyy). Campo <c>DT_FIN_REL</c>.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinRel { get; set; }
}
