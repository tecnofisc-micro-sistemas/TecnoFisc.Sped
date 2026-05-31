using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E001 — Abertura do Bloco E (Apuração do ICMS e do IPI).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 206.
/// </summary>
[RegistroSped(Codigo = "E001", Nivel = 1, Bloco = "E")]
public sealed partial class RegistroE001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
