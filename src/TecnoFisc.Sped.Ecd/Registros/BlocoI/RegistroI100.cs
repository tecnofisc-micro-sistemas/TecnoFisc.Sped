using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I100 — Centro de Custos. Nível hierárquico 3, ocorrência 0:N.
/// Obrigatório para todas as empresas que utilizem centros de custos em sua
/// contabilidade, mesmo que não tenham sido necessários nos registros I051 e I052.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 130–131.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_COD_CCUS_DUPLICADO</c>: <see cref="CodCcus"/> deve ser único
///   no arquivo (chave do registro).</item>
///   <item><c>REGRA_DT_ALT_DATA_MAIOR</c>: <see cref="DtAlt"/> deve ser menor ou
///   igual ao campo <c>DT_FIN</c> do Registro 0000.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I100", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I100";

    /// <summary>
    /// Data da inclusão/alteração do centro de custos (ddMMyyyy).
    /// Campo <c>DT_ALT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>
    /// Código do centro de custos. Deve ser único no arquivo.
    /// Campo <c>COD_CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Nome do centro de custos.
    /// Campo <c>CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? Ccus { get; set; }
}
