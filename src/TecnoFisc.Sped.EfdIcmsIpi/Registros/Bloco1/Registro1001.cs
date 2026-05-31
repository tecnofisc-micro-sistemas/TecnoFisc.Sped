using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1001 — Abertura do Bloco 1. Nível hierárquico 1, ocorrência única por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 267.
/// </summary>
[RegistroSped(Codigo = "1001", Nivel = 1, Bloco = "1")]
public sealed partial class Registro1001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
