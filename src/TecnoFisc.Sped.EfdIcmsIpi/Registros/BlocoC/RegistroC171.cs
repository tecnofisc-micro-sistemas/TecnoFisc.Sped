using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C171 — Armazenamento de Combustíveis (cód. 01, 55).
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 82.
/// </summary>
[RegistroSped(Codigo = "C171", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC171 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C171";

    /// <summary>Tanque onde foi armazenado o combustível.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public string? NumTanque { get; set; }

    /// <summary>Quantidade ou volume armazenado.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3)]
    public decimal? Qtde { get; set; }
}
