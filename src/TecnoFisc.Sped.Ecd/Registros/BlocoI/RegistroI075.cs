using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I075 — Tabela de Histórico Padronizado. Nível hierárquico 3, ocorrência 0:N.
/// A pessoa jurídica define históricos padronizados com códigos únicos para todo o período;
/// esses códigos são referenciados no campo <c>HIST</c> do Registro I250 (Partidas do Lançamento).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 129–130.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_DUPLICADO</c>: <see cref="CodHist"/> deve ser único para todo
///   o período a que se refere a escrituração.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I075", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI075 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I075";

    /// <summary>
    /// Código do histórico padronizado. Deve ser único para todo o período da escrituração.
    /// Campo <c>COD_HIST</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodHist { get; set; }

    /// <summary>
    /// Descrição do histórico padronizado.
    /// Campo <c>DESCR_HIST</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrHist { get; set; }
}
