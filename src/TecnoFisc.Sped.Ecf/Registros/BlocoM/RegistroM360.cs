using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M360 - contas contábeis relacionadas ao lançamento da Parte A do e-Lacs.</summary>
[RegistroSped(Codigo = "M360", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM360 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M360";

    /// <summary>Código da conta contábil relacionada ao lançamento.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Código opcional do centro de custos.</summary>
    [CampoSped(Ordem = 3)]
    public string? CodCcus { get; set; }

    /// <summary>Valor da conta utilizado no lançamento da Parte A.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCta { get; set; }

    /// <summary>Indicador devedor ou credor do valor da conta.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlCta { get; set; }
}
