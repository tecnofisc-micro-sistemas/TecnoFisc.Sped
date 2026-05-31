using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C052 — Indicação dos Códigos de Aglutinação Recuperados. Nível hierárquico 4, ocorrência 0:N.
/// Identifica os códigos de aglutinação referentes ao arquivo da ECD recuperado (Registro I052).
/// <c>COD_AGL</c> é o código utilizado nas demonstrações contábeis do bloco J; aplica-se somente
/// a contas analíticas.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 95.
/// </summary>
[RegistroSped(Codigo = "C052", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC052 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C052";

    /// <summary>Código do centro de custo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Código de aglutinação utilizado nas demonstrações contábeis do bloco J.
    /// Somente para contas analíticas.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodAgl { get; set; }
}
