using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

/// <summary>
/// Registro H010 — Inventário.
/// Nível hierárquico 3, ocorrência vários por registro H005. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 246-247.
/// </summary>
[RegistroSped(Codigo = "H010", Nivel = 3, Bloco = "H")]
public sealed partial class RegistroH010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "H010";

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Unidade do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Valor unitário do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal VlUnit { get; set; }

    /// <summary>Valor do item.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Indicador de propriedade ou posse do item.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPropriedadeItem IndProp { get; set; }

    /// <summary>Código do participante proprietário/possuidor que não seja o informante.</summary>
    [CampoSped(Ordem = 8, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Descrição complementar.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? TxtCompl { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? CodCta { get; set; }

    /// <summary>Valor do item para efeitos do Imposto de Renda.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlItemIr { get; set; }
}
