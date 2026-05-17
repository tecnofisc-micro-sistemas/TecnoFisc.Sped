using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K200 — Estoque Escriturado.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 250-251.
/// </summary>
[RegistroSped(Codigo = "K200", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K200";

    /// <summary>Data do estoque final (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtEst { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade em estoque.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Indicador do tipo de estoque.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPropriedadeItem IndEst { get; set; }

    /// <summary>Código do participante proprietário/possuidor que não seja o informante do arquivo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 60)]
    public string? CodPart { get; set; }
}
