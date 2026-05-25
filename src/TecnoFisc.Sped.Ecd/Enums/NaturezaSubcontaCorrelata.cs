using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Natureza da subconta correlata — campo <c>NAT_SUB_CNT</c> do Registro I053 da ECD.
/// Conforme Tabela de Natureza da Subconta publicada no Sped (Manual de Orientação do Leiaute 9
/// da ECD, p. 128–129).
/// </summary>
public enum NaturezaSubcontaCorrelata
{
    /// <summary>02 — Subconta TBU – Controlada Direta no Exterior (art. 76, Lei n.º 12.973/14).</summary>
    [SpedValor("02")]
    TbuControladaDiretaExterior = 2,

    /// <summary>03 — Subconta TBU – Controlada Indireta no Exterior (art. 76, Lei n.º 12.973/14).</summary>
    [SpedValor("03")]
    TbuControladaIndiretaExterior = 3,

    /// <summary>10 — Subconta Goodwill (art. 20, III, Decreto-Lei n.º 1.598/77).</summary>
    [SpedValor("10")]
    Goodwill = 10,

    /// <summary>11 — Subconta Mais Valia (art. 20, II, Decreto-Lei n.º 1.598/77).</summary>
    [SpedValor("11")]
    MaisValia = 11,

    /// <summary>12 — Subconta Menos Valia (art. 20, II, Decreto-Lei n.º 1.598/77).</summary>
    [SpedValor("12")]
    MenosValia = 12,

    /// <summary>60 — Subconta AVJ Reflexo (arts. 24A e 24B, Decreto-Lei n.º 1.598/77).</summary>
    [SpedValor("60")]
    AvjReflexo = 60,

    /// <summary>65 — Subconta AVJ Subscrição de Capital (arts. 17 e 18, Lei n.º 12.973/14).</summary>
    [SpedValor("65")]
    AvjSubscricaoCapital = 65,

    /// <summary>70 — Subconta AVJ – Vinculada Ativo/Passivo (arts. 13 e 14, Lei n.º 12.973/14).</summary>
    [SpedValor("70")]
    AvjVinculadaAtivoPassivo = 70,

    /// <summary>71 — Subconta AVJ – Depreciação Acumulada (arts. 13, §1.º, e 14, Lei n.º 12.973/14).</summary>
    [SpedValor("71")]
    AvjDepreciacaoAcumulada = 71,

    /// <summary>72 — Subconta AVJ – Amortização Acumulada (arts. 13, §1.º, e 14, Lei n.º 12.973/14).</summary>
    [SpedValor("72")]
    AvjAmortizacaoAcumulada = 72,

    /// <summary>73 — Subconta AVJ – Exaustão Acumulada (arts. 13, §1.º, e 14, Lei n.º 12.973/14).</summary>
    [SpedValor("73")]
    AvjExaustaoAcumulada = 73,

    /// <summary>75 — Subconta AVP – Vinculada ao Ativo (art. 5.º, §1.º, Lei n.º 12.973/14).</summary>
    [SpedValor("75")]
    AvpVinculadaAoAtivo = 75,

    /// <summary>76 — Subconta AVP – Depreciação Acumulada (art. 5.º, III, Lei n.º 12.973/14).</summary>
    [SpedValor("76")]
    AvpDepreciacaoAcumulada = 76,

    /// <summary>77 — Subconta AVP – Amortização Acumulada (art. 5.º, III, Lei n.º 12.973/14).</summary>
    [SpedValor("77")]
    AvpAmortizacaoAcumulada = 77,

    /// <summary>78 — Subconta AVP – Exaustão Acumulada (art. 5.º, III, Lei n.º 12.973/14).</summary>
    [SpedValor("78")]
    AvpExaustaoAcumulada = 78,

    /// <summary>80 — Subconta Mais Valia Anterior – Estágios (art. 37, §3.º, I, Lei n.º 12.973/14).</summary>
    [SpedValor("80")]
    MaisValiaAnteriorEstagios = 80,

    /// <summary>81 — Subconta Menos Valia Anterior – Estágios (art. 37, §3.º, I, Lei n.º 12.973/14).</summary>
    [SpedValor("81")]
    MenosValiaAnteriorEstagios = 81,

    /// <summary>82 — Subconta Goodwill Anterior – Estágios (art. 37, §3.º, I, Lei n.º 12.973/14).</summary>
    [SpedValor("82")]
    GoodwillAnteriorEstagios = 82,

    /// <summary>84 — Subconta Variação Mais Valia Anterior – Estágios (art. 37, §3.º, II, Lei n.º 12.973/14).</summary>
    [SpedValor("84")]
    VariacaoMaisValiaAnteriorEstagios = 84,

    /// <summary>85 — Subconta Variação Menos Valia Anterior – Estágios (art. 37, §3.º, II, Lei n.º 12.973/14).</summary>
    [SpedValor("85")]
    VariacaoMenosValiaAnteriorEstagios = 85,

    /// <summary>86 — Subconta Variação Goodwill Anterior – Estágios (art. 37, §3.º, II, Lei n.º 12.973/14).</summary>
    [SpedValor("86")]
    VariacaoGoodwillAnteriorEstagios = 86,

    /// <summary>90 — Subconta Adoção Inicial – Vinculada ou Auxiliar – Ativo/Passivo
    /// (arts. 66 e 67, Lei n.º 12.973/14; IN RFB n.º 1.700/2017).</summary>
    [SpedValor("90")]
    AdocaoInicialAtivoPassivo = 90,

    /// <summary>91 — Subconta Adoção Inicial – Vinculada ou Auxiliar – Depreciação Acumulada
    /// (arts. 66 e 67, Lei n.º 12.973/14; IN RFB n.º 1.700/2017).</summary>
    [SpedValor("91")]
    AdocaoInicialDepreciacaoAcumulada = 91,

    /// <summary>92 — Subconta Adoção Inicial – Vinculada ou Auxiliar – Amortização Acumulada
    /// (arts. 66 e 67, Lei n.º 12.973/14; IN RFB n.º 1.700/2017).</summary>
    [SpedValor("92")]
    AdocaoInicialAmortizacaoAcumulada = 92,

    /// <summary>93 — Subconta Adoção Inicial – Vinculada ou Auxiliar – Exaustão Acumulada
    /// (arts. 66 e 67, Lei n.º 12.973/14; IN RFB n.º 1.700/2017).</summary>
    [SpedValor("93")]
    AdocaoInicialExaustaoAcumulada = 93,
}
