using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K280 — Correção de Apontamento — Estoque Escriturado.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 262-263.
/// </summary>
[RegistroSped(Codigo = "K280", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK280 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K280";

    /// <summary>Data do estoque final escriturado que está sendo corrigido (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtEst { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade de correção positiva de apontamento ocorrido em período de apuração anterior.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3)]
    public decimal? QtdCorPos { get; set; }

    /// <summary>Quantidade de correção negativa de apontamento ocorrido em período de apuração anterior.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? QtdCorNeg { get; set; }

    /// <summary>Indicador do tipo de estoque.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPropriedadeItem IndEst { get; set; }

    /// <summary>Código do participante proprietário/possuidor que não seja o informante do arquivo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 60)]
    public string? CodPart { get; set; }
}
