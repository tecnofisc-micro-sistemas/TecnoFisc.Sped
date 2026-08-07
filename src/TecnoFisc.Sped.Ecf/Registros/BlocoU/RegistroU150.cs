using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U150 - demonstração do resultado por conta referencial.</summary>
[RegistroSped(Codigo = "U150", Nivel = 3, Bloco = "U")]
public sealed partial class RegistroU150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U150";

    /// <summary>Código da conta referencial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 50, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da conta referencial.</summary>
    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Tipo analítico ou sintético da conta.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "TIPO")]
    public IndicadorTipoConta Tipo { get; set; }

    /// <summary>Nível da conta no plano referencial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Nome = "NIVEL")]
    public int? Nivel { get; set; }

    /// <summary>Natureza da conta, preservada com zeros significativos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Nome = "COD_NAT")]
    public string? CodNat { get; set; }

    /// <summary>Código da conta sintética imediatamente superior.</summary>
    [CampoSped(Ordem = 7, Nome = "COD_CTA_SUP")]
    public string? CodCtaSup { get; set; }

    /// <summary>Saldo final da conta referencial.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR")]
    public decimal Valor { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_VALOR")]
    public IndicadorDebitoCredito IndValor { get; set; }
}
