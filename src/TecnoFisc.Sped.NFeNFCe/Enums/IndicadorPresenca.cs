namespace TecnoFisc.Sped.NFeNFCe.Enums;

/// <summary>Indicador de presença do comprador no momento da operação — campo indPres do grupo ide.</summary>
public enum IndicadorPresenca
{
    /// <summary>0 — Não se aplica (ex.: nota complementar ou de ajuste).</summary>
    NaoSeAplica = 0,

    /// <summary>1 — Operação presencial.</summary>
    Presencial = 1,

    /// <summary>2 — Operação não presencial, pela internet.</summary>
    Internet = 2,

    /// <summary>3 — Operação não presencial, teleatendimento.</summary>
    Teleatendimento = 3,

    /// <summary>4 — NFC-e em operação com entrega a domicílio.</summary>
    NfceEntregaDomicilio = 4,

    /// <summary>5 — Operação presencial, fora do estabelecimento.</summary>
    PresencialForaEstabelecimento = 5,

    /// <summary>9 — Operação não presencial, outros.</summary>
    NaoPresencialOutros = 9,
}
