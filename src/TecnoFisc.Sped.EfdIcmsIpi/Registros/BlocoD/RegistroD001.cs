using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D001 — Abertura do Bloco D (Documentos Fiscais II — Serviços ICMS).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 164.
/// </summary>
[RegistroSped(Codigo = "D001", Nivel = 1, Bloco = "D")]
public sealed partial class RegistroD001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
