using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

/// <summary>
/// Registro H005 — Totais do Inventário.
/// Nível hierárquico 2, ocorrência vários por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 245-246.
/// </summary>
[RegistroSped(Codigo = "H005", Nivel = 2, Bloco = "H")]
public sealed partial class RegistroH005 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "H005";

    /// <summary>Data do inventário (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtInv { get; set; }

    /// <summary>Valor total do estoque.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlInv { get; set; }

    /// <summary>Motivo do inventário.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public MotivoInventario MotInv { get; set; }
}
