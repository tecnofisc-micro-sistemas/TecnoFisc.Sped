using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U100 - balanço patrimonial por conta referencial.</summary>
[RegistroSped(Codigo = "U100", Nivel = 3, Bloco = "U")]
public sealed partial class RegistroU100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U100";

    /// <summary>Código da conta referencial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 50, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da conta referencial.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Tipo analítico ou sintético da conta.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta Tipo { get; set; }

    /// <summary>Nível da conta no plano referencial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Nivel { get; set; }

    /// <summary>Código da natureza da conta, preservado com zeros significativos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2)]
    public string? CodNat { get; set; }

    /// <summary>Código da conta sintética imediatamente superior.</summary>
    [CampoSped(Ordem = 7)]
    public string? CodCtaSup { get; set; }

    /// <summary>Saldo inicial da conta referencial.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValCtaRefIni { get; set; }

    /// <summary>Natureza devedora ou credora do saldo inicial.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValCtaRefIni { get; set; }

    /// <summary>Total dos débitos mapeados.</summary>
    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValCtaRefDeb { get; set; }

    /// <summary>Total dos créditos mapeados.</summary>
    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValCtaRefCred { get; set; }

    /// <summary>Saldo final na representação textual declarada pelo leiaute.</summary>
    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public string? ValCtaRefFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValCtaRefFin { get; set; }
}
