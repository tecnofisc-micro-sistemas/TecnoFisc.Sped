using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K156 - mapeamento referencial do saldo patrimonial.</summary>
[RegistroSped(Codigo = "K156", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK156 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K156";

    /// <summary>Código da conta no plano referencial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA_REF")]
    public string? CodCtaRef { get; set; }

    /// <summary>Valor do saldo inicial.</summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_INI")]
    public decimal VlSldIni { get; set; }

    /// <summary>Natureza devedora ou credora do saldo inicial.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_INI")]
    public IndicadorDebitoCredito IndVlSldIni { get; set; }

    /// <summary>Total de débitos do período.</summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_DEB")]
    public decimal VlDeb { get; set; }

    /// <summary>Total de créditos do período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_CRED")]
    public decimal VlCred { get; set; }

    /// <summary>Valor do saldo final.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_FIN")]
    public decimal VlSldFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_FIN")]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
