using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo de relacionamento da participação avaliada pelo MEP no registro Y620.</summary>
public enum TipoRelacionamentoParticipacao
{
    /// <summary>1 - controle.</summary>
    [SpedValor("1")]
    Controle = 1,

    /// <summary>2 - controle conjunto.</summary>
    [SpedValor("2")]
    ControleConjunto = 2,

    /// <summary>3 - influência significativa.</summary>
    [SpedValor("3")]
    InfluenciaSignificativa = 3,

    /// <summary>4 - outras causas para aplicar MEP.</summary>
    [SpedValor("4")]
    OutrasCausasMep = 4,

    /// <summary>5 - exclusão do MEP.</summary>
    [SpedValor("5")]
    ExclusaoMep = 5,
}
