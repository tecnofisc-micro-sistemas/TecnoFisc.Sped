using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoL;

/// <summary>Registro L100 - balanço patrimonial por conta referencial.</summary>
[RegistroSped(Codigo = "L100", Nivel = 3, Bloco = "L")]
public sealed partial class RegistroL100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "L100";

    /// <summary>Código da conta referencial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 50, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da conta referencial.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Tipo analítico ou sintético da conta.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "TIPO")]
    public IndicadorTipoConta Tipo { get; set; }

    /// <summary>Nível da conta no plano referencial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Nome = "NIVEL")]
    public int? Nivel { get; set; }

    /// <summary>Código da natureza da conta, preservado com zeros significativos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Nome = "COD_NAT")]
    public string? CodNat { get; set; }

    /// <summary>Código da conta sintética imediatamente superior.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Nome = "COD_CTA_SUP")]
    public string? CodCtaSup { get; set; }

    /// <summary>Saldo inicial da conta referencial.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA_REF_INI")]
    public decimal ValCtaRefIni { get; set; }

    /// <summary>Natureza devedora ou credora do saldo inicial.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_VAL_CTA_REF_INI")]
    public IndicadorDebitoCredito IndValCtaRefIni { get; set; }

    /// <summary>Total dos débitos mapeados.</summary>
    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA_REF_DEB")]
    public decimal ValCtaRefDeb { get; set; }

    /// <summary>Total dos créditos mapeados.</summary>
    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA_REF_CRED")]
    public decimal ValCtaRefCred { get; set; }

    /// <summary>
    /// Saldo final na representação textual declarada pelo leiaute 12, preservado sem
    /// executar o cálculo fiscal descrito pelo manual.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA_REF_FIN")]
    public string? ValCtaRefFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true, Nome = "IND_VAL_CTA_REF_FIN")]
    public IndicadorDebitoCredito IndValCtaRefFin { get; set; }
}
