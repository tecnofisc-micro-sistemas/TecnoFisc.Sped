using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C150 — Saldos Periódicos Recuperados – Identificação do Período. Nível hierárquico 3, ocorrência 1:1.
/// Identifica o período dos saldos periódicos recuperados da ECD anterior.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 96.
/// </summary>
[RegistroSped(Codigo = "C150", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C150";

    /// <summary>Data inicial do período.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
