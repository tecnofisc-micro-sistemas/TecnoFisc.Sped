using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W300 - observações adicionais da Declaração País-a-País.</summary>
[RegistroSped(Codigo = "W300", Nivel = 2, Bloco = "W")]
public sealed partial class RegistroW300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W300";

    [CampoSped(Ordem = 2)]
    public string? Jurisdicao { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1)]
    public IndicadorSimNao? IndRecNaoRel { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorSimNao? IndRecRel { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorSimNao? IndRecTotal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1)]
    public IndicadorSimNao? IndLucPrejAntesIr { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorSimNao? IndIrPago { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1)]
    public IndicadorSimNao? IndIrDevido { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1)]
    public IndicadorSimNao? IndCapSoc { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 1)]
    public IndicadorSimNao? IndLucAcum { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 1)]
    public IndicadorSimNao? IndAtivTang { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 1)]
    public IndicadorSimNao? IndNumEmp { get; set; }

    [CampoSped(Ordem = 13, Nome = "OBSERVAÇÃO", Tamanho = 4000, Obrigatorio = true)]
    public string? Observação { get; set; }

    /// <summary>Marcador fixo W300FIM, obrigatório conforme o marcador normativo "sim".</summary>
    [CampoSped(Ordem = 14, Tamanho = 7, Obrigatorio = true)]
    public string? FimObservacao { get; set; }
}
