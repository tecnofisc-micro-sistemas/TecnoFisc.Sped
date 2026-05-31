using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D360 — PIS e COFINS Totalizados no Dia para ECF de Bilhetes de Passagem
/// (cód. 2E, 13, 14, 15 e 16).
/// Nível hierárquico 4, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 185.
/// </summary>
[RegistroSped(Codigo = "D360", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD360 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D360";

    /// <summary>Valor total do PIS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
