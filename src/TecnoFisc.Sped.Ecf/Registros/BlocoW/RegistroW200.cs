using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W200 - valores agregados da Declaração País-a-País.</summary>
[RegistroSped(Codigo = "W200", Nivel = 3, Bloco = "W")]
public sealed partial class RegistroW200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W200";

    /// <summary>Jurisdição de residência conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? Jurisdicao { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 0)]
    public decimal? VlRecNaoRelEst { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlRecNaoRel { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 0)]
    public decimal? VlRecRelEst { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlRecRel { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 0)]
    public decimal? VlRecTotalEst { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlRecTotal { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 0)]
    public decimal? VlLucPrejAntesIrEst { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlLucPrejAntesIr { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 0)]
    public decimal? VlIrPagoEst { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlIrPago { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 0)]
    public decimal? VlIrDevidoEst { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlIrDevido { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 0)]
    public decimal? VlCapSocEst { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlCapSoc { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 19, Decimais = 0)]
    public decimal? VlLucAcumEst { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlLucAcum { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 19, Decimais = 0)]
    public decimal? VlAtivTangEst { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 19, Decimais = 0, Obrigatorio = true)]
    public decimal VlAtivTang { get; set; }

    /// <summary>Número de empregados.</summary>
    [CampoSped(Ordem = 21, Tamanho = 7, Decimais = 0, Obrigatorio = true)]
    public int NumEmp { get; set; }
}
