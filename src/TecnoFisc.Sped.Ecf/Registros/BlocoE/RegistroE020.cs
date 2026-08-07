using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E020 - saldos finais das contas da Parte B da ECF anterior.</summary>
[RegistroSped(Codigo = "E020", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E020";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "DESC_CTA_LAL")]
    public string? DescCtaLal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_AP_LAL")]
    public DateOnly? DtApLal { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_LIM_LAL")]
    public DateOnly? DtLimLal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Nome = "TRIBUTO")]
    public IndicadorTributoParteB? Tributo { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Nome = "VL_SALDO_FIN")]
    public decimal? VlSaldoFin { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1, Nome = "IND_VL_SALDO_FIN")]
    public IndicadorDebitoCredito? IndVlSaldoFin { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 6, Obrigatorio = true, Nome = "COD_PB_RFB")]
    public string? CodPbRfb { get; set; }
}
