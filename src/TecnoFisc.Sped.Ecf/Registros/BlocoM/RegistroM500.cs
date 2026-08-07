using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M500 - controle de saldos das contas da Parte B.</summary>
[RegistroSped(Codigo = "M500", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M500";

    /// <summary>Código unívoco da conta da Parte B atribuído pelo contribuinte.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    /// <summary>Tributo associado à conta.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true, Nome = "COD_TRIBUTO")]
    public IndicadorTributoContaParteB CodTributo { get; set; }

    /// <summary>Saldo inicial da conta no período de apuração.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SD_INI_LAL")]
    public decimal SdIniLal { get; set; }

    /// <summary>Indicador do saldo inicial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_SD_INI_LAL")]
    public IndicadorDebitoCredito IndSdIniLal { get; set; }

    /// <summary>Somatório dos lançamentos da Parte B com reflexo na Parte A.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_LCTO_PARTE_A")]
    public decimal VlLctoParteA { get; set; }

    /// <summary>Indicador dos lançamentos da Parte B com reflexo na Parte A.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_LCTO_PARTE_A")]
    public IndicadorDebitoCredito IndVlLctoParteA { get; set; }

    /// <summary>Somatório dos lançamentos da Parte B sem reflexo na Parte A.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_LCTO_PARTEB")]
    public decimal VlLctoParteB { get; set; }

    /// <summary>Indicador dos lançamentos da Parte B sem reflexo na Parte A.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_LCTO_PARTEB")]
    public IndicadorDebitoCredito IndVlLctoParteB { get; set; }

    /// <summary>Saldo final da conta no período de apuração.</summary>
    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SD_FIM_LAL")]
    public decimal SdFimLal { get; set; }

    /// <summary>Indicador do saldo final.</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true, Nome = "IND_SD_FIM_LAL")]
    public IndicadorDebitoCredito IndSdFimLal { get; set; }
}
