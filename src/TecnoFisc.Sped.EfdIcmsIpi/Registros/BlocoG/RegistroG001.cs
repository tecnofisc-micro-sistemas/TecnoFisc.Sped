using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G001 — Abertura do Bloco G (Controle do Crédito de ICMS do Ativo Permanente CIAP).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 236.
/// </summary>
[RegistroSped(Codigo = "G001", Nivel = 1, Bloco = "G")]
public sealed partial class RegistroG001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
