using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K155 - saldos contábeis patrimoniais do período.</summary>
[RegistroSped(Codigo = "K155", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK155 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K155";

    /// <summary>Código da conta analítica patrimonial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Valor do saldo inicial.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_INI")]
    public decimal VlSldIni { get; set; }

    /// <summary>Natureza devedora ou credora do saldo inicial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_INI")]
    public IndicadorDebitoCredito IndVlSldIni { get; set; }

    /// <summary>Total de débitos do período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_DEB")]
    public decimal VlDeb { get; set; }

    /// <summary>Total de créditos do período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_CRED")]
    public decimal VlCred { get; set; }

    /// <summary>Valor do saldo final.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_FIN")]
    public decimal VlSldFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_FIN")]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
