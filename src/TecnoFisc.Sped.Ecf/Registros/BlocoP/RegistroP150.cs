using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoP;

/// <summary>Registro P150 - resultado líquido por conta referencial.</summary>
[RegistroSped(Codigo = "P150", Nivel = 3, Bloco = "P")]
public sealed partial class RegistroP150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P150";

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

    /// <summary>Natureza da conta, preservada com zeros significativos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2)]
    public string? CodNat { get; set; }

    /// <summary>Código da conta sintética imediatamente superior.</summary>
    [CampoSped(Ordem = 7)]
    public string? CodCtaSup { get; set; }

    /// <summary>Saldo final da conta referencial.</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal Valor { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValor { get; set; }
}
