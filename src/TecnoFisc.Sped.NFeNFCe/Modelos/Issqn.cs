using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>ISSQN</c> — dados do Imposto Sobre Serviços de Qualquer Natureza de um item.
/// Elemento irmão de <c>ICMS</c>/<c>IPI</c> dentro de <c>imposto</c>; mutuamente exclusivo com
/// <c>ICMS</c> na prática (produto vs. serviço), mas ambos são opcionais no modelo de leitura.
/// Não possui hierarquia polimórfica: é um registro plano.
/// </summary>
public sealed record Issqn
{
    // -------------------------------------------------------------------------
    // Campos obrigatórios
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do ISSQN.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>vAliq</c> — alíquota do ISSQN (em percentual).</summary>
    public required decimal VAliq { get; init; }

    /// <summary><c>vISSQN</c> — valor do ISSQN.</summary>
    public required decimal VISSQN { get; init; }

    /// <summary>
    /// <c>cMunFG</c> — município de ocorrência do fato gerador do ISSQN (código IBGE).
    /// Não vincular com os campos <c>B12</c>, <c>C10</c> ou <c>E10</c> do leiaute.
    /// </summary>
    public required CodigoMunicipioIbge CMunFG { get; init; }

    /// <summary>
    /// <c>cListServ</c> — item da lista de serviços da LC 116/2003 em que se classifica o serviço.
    /// </summary>
    public required string CListServ { get; init; }

    /// <summary>
    /// <c>indISS</c> — exigibilidade do ISS:
    /// 1=Exigível; 2=Não incidente; 3=Isenção; 4=Exportação; 5=Imunidade;
    /// 6=Exig.Susp. Judicial; 7=Exig.Susp. ADM.
    /// </summary>
    // TODO: candidato a enum tipado (ExigibilidadeIss) quando houver camada de validação.
    public required int IndISS { get; init; }

    /// <summary><c>indIncentivo</c> — indicador de incentivo fiscal: 1=Sim; 2=Não.</summary>
    // TODO: candidato a enum tipado (IndicadorIncentivo) quando houver camada de validação.
    public required int IndIncentivo { get; init; }

    // -------------------------------------------------------------------------
    // Campos opcionais (minOccurs="0")
    // -------------------------------------------------------------------------

    /// <summary><c>vDeducao</c> — valor da dedução para redução da base de cálculo (opcional).</summary>
    public decimal? VDeducao { get; init; }

    /// <summary><c>vOutro</c> — valor de outras retenções (opcional).</summary>
    public decimal? VOutro { get; init; }

    /// <summary><c>vDescIncond</c> — valor do desconto incondicionado (opcional).</summary>
    public decimal? VDescIncond { get; init; }

    /// <summary><c>vDescCond</c> — valor do desconto condicionado (opcional).</summary>
    public decimal? VDescCond { get; init; }

    /// <summary><c>vISSRet</c> — valor da retenção do ISS (opcional).</summary>
    public decimal? VISSRet { get; init; }

    /// <summary><c>cServico</c> — código do serviço prestado dentro do município (opcional).</summary>
    public string? CServico { get; init; }

    /// <summary><c>cMun</c> — código IBGE do município de incidência do imposto (opcional).</summary>
    public CodigoMunicipioIbge? CMun { get; init; }

    /// <summary><c>cPais</c> — código do país (opcional; 1–4 dígitos numéricos, ex.: 1058 = Brasil).</summary>
    public int? CPais { get; init; }

    /// <summary><c>nProcesso</c> — número do processo administrativo ou judicial de suspensão (opcional).</summary>
    public string? NProcesso { get; init; }
}
