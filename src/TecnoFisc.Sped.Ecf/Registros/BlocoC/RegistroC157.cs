using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C157 - transferência de saldos do plano anterior.</summary>
[RegistroSped(Codigo = "C157", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC157 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C157";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorDebitoCredito? IndVlSldFin { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public int LinhaEcd { get; set; }
}
