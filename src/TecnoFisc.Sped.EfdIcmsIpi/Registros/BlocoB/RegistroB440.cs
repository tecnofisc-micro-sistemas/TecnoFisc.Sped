using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B440 — Totalização dos Valores Retidos.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 53.
/// </summary>
[RegistroSped(Codigo = "B440", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB440 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B440";

    /// <summary>Indicador do tipo de operação: 0 Aquisição, 1 Prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoIss IndOper { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): prestador na aquisição, tomador na prestação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Totalização do Valor Contábil pela combinação de tipo de operação e participante.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContRt { get; set; }

    /// <summary>Totalização do Valor da base de cálculo de retenção do ISS pela combinação de tipo de operação e participante.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIssRt { get; set; }

    /// <summary>Totalização do Valor do ISS retido pelo tomador pela combinação de tipo de operação e participante.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssRt { get; set; }
}
