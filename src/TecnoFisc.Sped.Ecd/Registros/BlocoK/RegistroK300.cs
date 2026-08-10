using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K300 — Saldos das Contas Consolidadas. Nível hierárquico 3, ocorrência 0:N por arquivo.
/// Apresenta os saldos das contas consolidadas.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 223–225.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OBRIGATORIEDADE_K310</c>: quando <see cref="ValEl"/> for maior que zero, deve
///   existir pelo menos um registro K310 filho. O PGE gera erro caso contrário.</item>
///   <item><c>REGRA_EXISTE_K200_ANALITICA</c>: <see cref="CodCta"/> deve existir no registro K200
///   e a conta deve ser analítica (<c>IND_CTA = "A"</c>). O PGE gera erro caso contrário.</item>
///   <item><c>REGRA_SOMATORIO_VALOR_ELIMINACOES</c>: <see cref="ValEl"/> deve ser igual ao
///   somatório das parcelas do valor eliminado total — VALOR (Campo 03) do registro K310,
///   considerando o indicador da situação do saldo (devedor ou credor). O PGE gera erro caso
///   contrário.</item>
///   <item><c>REGRA_CALCULO_VALOR_CONSOLIDADO</c>: <see cref="ValCs"/> deve ser igual à diferença
///   entre <see cref="ValAg"/> e <see cref="ValEl"/>, considerando os indicadores da situação do
///   saldo. O PGE gera erro caso contrário.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K300", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K300";

    /// <summary>
    /// Código da conta consolidada.
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Valor absoluto aglutinado.
    /// Campo <c>VAL_AG</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValAg { get; set; }

    /// <summary>
    /// Indicador da situação do valor aglutinado: D – Devedor; C – Credor.
    /// Campo <c>IND_VAL_AG</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValAg { get; set; }

    /// <summary>
    /// Valor absoluto das eliminações.
    /// Campo <c>VAL_EL</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValEl { get; set; }

    /// <summary>
    /// Indicador da situação do valor eliminado: D – Devedor; C – Credor.
    /// Campo <c>IND_VAL_EL</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValEl { get; set; }

    /// <summary>
    /// Valor absoluto consolidado: VAL_CS = VAL_AG – VAL_EL.
    /// Campo <c>VAL_CS</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValCs { get; set; }

    /// <summary>
    /// Indicador da situação do valor consolidado: D – Devedor; C – Credor.
    /// Campo <c>IND_VAL_CS</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndValCs { get; set; }
}
