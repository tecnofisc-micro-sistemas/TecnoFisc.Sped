using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E155 - detalhes dos saldos contábeis calculados com base nas ECD.</summary>
[RegistroSped(Codigo = "E155", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE155 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E155";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldIni { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlSldIni { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDeb { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCred { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFin { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
