using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D350 — Equipamento ECF utilizado na emissão de Cupom Fiscal Bilhete de Passagem (2E),
/// Bilhete de Passagem Rodoviário (13), Aquaviário (14), Passagem e Nota de Bagagem (15) e Ferroviário (16).
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 184.
/// </summary>
[RegistroSped(Codigo = "D350", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D350";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 2E, 13, 14, 15 e 16.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Modelo do equipamento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? EcfMod { get; set; }

    /// <summary>Número de série de fabricação do ECF.</summary>
    [CampoSped(Ordem = 4, Tamanho = 21, Obrigatorio = true)]
    public string? EcfFab { get; set; }

    /// <summary>Número do caixa atribuído ao ECF pelo estabelecimento.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public int? EcfCx { get; set; }
}
