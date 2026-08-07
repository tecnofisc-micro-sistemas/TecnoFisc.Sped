using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M410 - lançamento em conta da Parte B sem reflexo na Parte A.</summary>
[RegistroSped(Codigo = "M410", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM410 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M410";

    /// <summary>Código opcional da conta da Parte B.</summary>
    [CampoSped(Ordem = 2, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    /// <summary>Tributo associado ao lançamento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true, Nome = "COD_TRIBUTO")]
    public IndicadorTributoContaParteB CodTributo { get; set; }

    /// <summary>Valor do lançamento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_LAN_LALB_PB")]
    public decimal ValLanLalbPb { get; set; }

    /// <summary>Indicador do lançamento.</summary>
    [CampoSped(Ordem = 5, Obrigatorio = true, Nome = "IND_VAL_LAN_LALB_PB")]
    public IndicadorLancamentoParteB IndValLanLalbPb { get; set; }

    /// <summary>Código opcional da conta da Parte B de contrapartida.</summary>
    [CampoSped(Ordem = 6, Nome = "COD_CTA_B_CTP")]
    public string? CodCtaBCtp { get; set; }

    /// <summary>Histórico do lançamento.</summary>
    [CampoSped(Ordem = 7, Obrigatorio = true, Nome = "HIST_LAN_LALB")]
    public string? HistLanLalb { get; set; }

    /// <summary>Indica lançamento relativo à realização de valores diferidos.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true, Nome = "IND_LAN_ANT")]
    public IndicadorSimNao IndLanAnt { get; set; }
}
