using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0111 — Tabela de Receita Bruta Mensal Para Fins de Rateio de Créditos Comuns.
/// Nível hierárquico 3, ocorrência 1:1. Obrigatório quando Registro 0110 campo 03
/// (IND_APRO_CRED) indica rateio proporcional pela Receita Bruta (indicador "2").
/// Validação: REC_BRU_TOTAL deve ser igual à soma dos campos 02 a 05.
/// Conforme Guia Prático v1.35, p. 74.
/// </summary>
[RegistroSped(Codigo = "0111", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0111 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0111";

    /// <summary>Receita Bruta Não-Cumulativa tributada no Mercado Interno.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecBruNcumTribMi { get; set; }

    /// <summary>Receita Bruta Não-Cumulativa não tributada no Mercado Interno (suspensão, alíquota zero, isenção, não incidência).</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecBruNcumNtMi { get; set; }

    /// <summary>Receita Bruta Não-Cumulativa de Exportação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecBruNcumExp { get; set; }

    /// <summary>Receita Bruta Cumulativa.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecBruCum { get; set; }

    /// <summary>Receita Bruta Total (deve ser igual à soma dos campos 02 a 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecBruTotal { get; set; }
}
