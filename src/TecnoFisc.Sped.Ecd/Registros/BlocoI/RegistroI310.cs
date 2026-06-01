using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I310 — Detalhes do Balancete Diário. Nível hierárquico 4, ocorrência 0:N por I300.
/// Informa os totais de débitos e créditos de cada conta analítica/centro de custos em determinada data.
/// Utilizado apenas quando o tipo de escrituração é "B" (Livro de Balancetes Diários e Balanços),
/// conforme <c>IND_ESC</c> do Registro I010.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 153–155.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_DETALHE_BALANCETE_DUPLICADO</c>: a chave <see cref="CodCta"/>+<see cref="CodCcus"/>
///   não pode ser duplicada para o mesmo I300.</item>
///   <item><c>REGRA_VALIDACAO_DC_BALANCETE</c>: a soma de <see cref="ValDebd"/> de todas as
///   contas/centros de custo deve ser igual à soma de <see cref="ValCredd"/> na mesma data do I300.</item>
///   <item><c>REGRA_CONTA_PARA_LANCAMENTO</c>: <see cref="CodCta"/> deve ser conta analítica
///   (<c>IND_CTA = "A"</c>) existente no plano de contas (Registro I050).</item>
///   <item><c>REGRA_CCUS_NO_CENTRO_CUSTOS</c>: quando preenchido, <see cref="CodCcus"/> deve
///   existir no Registro I100 (Centro de Custos).</item>
///   <item>Campos adicionais de moeda funcional (<c>VAL_DEB_MF</c>, <c>VAL_CRED_MF</c>) são criados
///   via Registro I020 e presentes somente quando <c>IDENT_MF = "S"</c> no Registro 0000.
///   Não fazem parte do leiaute fixo — o parser os descarta silenciosamente.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I310", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I310";

    /// <summary>
    /// Código da conta analítica debitada/creditada. Chave do registro (junto com <see cref="CodCcus"/>).
    /// Campo <c>COD_CTA</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>
    /// Código do centro de custos.
    /// Campo <c>COD_CCUS</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Total dos débitos do dia.
    /// Campo <c>VAL_DEBD</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValDebd { get; set; }

    /// <summary>
    /// Total dos créditos do dia.
    /// Campo <c>VAL_CREDD</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ValCredd { get; set; }
}
