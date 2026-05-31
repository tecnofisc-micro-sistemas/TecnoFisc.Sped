using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C410 — PIS e COFINS Totalizados no Dia (código 02 e 2D).
/// Apresentado quando houver produtos totalizados na Redução Z que acarretem valores de
/// PIS e COFINS. Contribuintes entregando EFD-Contribuições do mesmo período estão dispensados.
/// Nível hierárquico 4, ocorrência 1:1 por C405.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 120.
/// </summary>
[RegistroSped(Codigo = "C410", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC410 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C410";

    /// <summary>Valor total do PIS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
