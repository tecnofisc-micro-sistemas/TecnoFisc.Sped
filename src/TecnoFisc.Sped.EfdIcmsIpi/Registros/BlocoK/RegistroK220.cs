using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K220 — Outras Movimentações Internas entre Mercadorias.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 253-254.
/// </summary>
[RegistroSped(Codigo = "K220", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK220 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K220";

    /// <summary>Data da movimentação interna (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtMov { get; set; }

    /// <summary>Código do item de origem (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemOri { get; set; }

    /// <summary>Código do item de destino (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemDest { get; set; }

    /// <summary>Quantidade movimentada do item de origem.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdOri { get; set; }

    /// <summary>Quantidade movimentada do item de destino.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdDest { get; set; }
}
