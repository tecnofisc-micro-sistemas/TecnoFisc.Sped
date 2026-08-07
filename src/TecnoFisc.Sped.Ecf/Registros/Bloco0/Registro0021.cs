using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0021 — parâmetros de identificação dos tipos de programa.</summary>
[RegistroSped(Codigo = "0021", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0021 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0021";

    [CampoSped(Ordem = 2, Tamanho = 1, Nome = "IND_REPES")]
    public IndicadorSimNao? IndRepes { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Nome = "IND_RECAP")]
    public IndicadorSimNao? IndRecap { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Nome = "IND_PADIS")]
    public IndicadorSimNao? IndPadis { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "IND_REIDI")]
    public IndicadorSimNao? IndReidi { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Nome = "IND_RECINE")]
    public IndicadorSimNao? IndRecine { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 1, Nome = "IND_RETID")]
    public IndicadorSimNao? IndRetid { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1, Nome = "IND_OLEO_BUNKER")]
    public IndicadorSimNao? IndOleoBunker { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1, Nome = "IND_REPORTO")]
    public IndicadorSimNao? IndReporto { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 1, Nome = "IND_RET_II")]
    public IndicadorSimNao? IndRetIi { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 1, Nome = "IND_RET_PMCMV")]
    public IndicadorSimNao? IndRetPmcmv { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 1, Nome = "IND_RET_EEI")]
    public IndicadorSimNao? IndRetEei { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Nome = "IND_EBAS")]
    public IndicadorSimNao? IndEbas { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 1, Nome = "IND_REPETRO_INDUSTRIALIZACAO")]
    public IndicadorSimNao? IndRepetroIndustrializacao { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 1, Nome = "IND_REPETRO_NACIONAL")]
    public IndicadorSimNao? IndRepetroNacional { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 1, Nome = "IND_REPETRO_PERMANENTE")]
    public IndicadorSimNao? IndRepetroPermanente { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 1, Nome = "IND_REPETRO_TEMPORARIO")]
    public IndicadorSimNao? IndRepetroTemporario { get; set; }
}
