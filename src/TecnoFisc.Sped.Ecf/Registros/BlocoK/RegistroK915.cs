using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K915 - dados da divergência de saldos patrimoniais.</summary>
[RegistroSped(Codigo = "K915", Nivel = 2, Bloco = "K")]
public sealed partial class RegistroK915 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K915";

    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true, Nome = "PER_APUR")]
    public string? PerApur { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true, Nome = "ID_REGRA")]
    public string? IdRegra { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Nome = "VL_SLD_INI_ESP")]
    public decimal? VlSldIniEsp { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 1, Nome = "IND_VL_SLD_INI_ESP")]
    public IndicadorDebitoCredito? IndVlSldIniEsp { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Nome = "VL_DEB_ESP")]
    public decimal? VlDebEsp { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Nome = "VL_CRED_ESP")]
    public decimal? VlCredEsp { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Nome = "VL_SLD_FIN_ESP")]
    public decimal? VlSldFinEsp { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 1, Nome = "IND_VL_SLD_FIN_ESP")]
    public IndicadorDebitoCredito? IndVlSldFinEsp { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Nome = "SLD_INI_PRE")]
    public decimal? SldIniPre { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Nome = "IND_SLD_INI_PRE")]
    public IndicadorDebitoCredito? IndSldIniPre { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Nome = "VL_DEB_PRE")]
    public decimal? VlDebPre { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Nome = "VL_CRED_PRE")]
    public decimal? VlCredPre { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Nome = "SLD_FIN_PRE")]
    public decimal? SldFinPre { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 1, Nome = "IND_SLD_FIN_PRE")]
    public IndicadorDebitoCredito? IndSldFinPre { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 500, Obrigatorio = true, Nome = "JUSTIFICATIVA")]
    public string? Justificativa { get; set; }
}
