using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E300 — Período de Apuração do Fundo de Combate à Pobreza e do ICMS Diferencial de Alíquota — UF Origem/Destino EC 87/15.
/// Nível hierárquico 2, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 236-237.
/// </summary>
[RegistroSped(Codigo = "E300", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E300";

    /// <summary>Sigla da unidade da federação a que se refere à apuração do FCP e do ICMS Diferencial de Alíquota da UF de Origem/Destino.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
