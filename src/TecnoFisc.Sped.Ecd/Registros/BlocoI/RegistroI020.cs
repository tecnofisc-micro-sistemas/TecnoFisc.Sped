using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I020 — Campos Adicionais. Nível hierárquico 3, ocorrência 0:N.
/// Utilizado quando for necessário informar dados não previstos nos arquivos padronizados,
/// acrescidos ao final dos registros de I050 a I355.
/// Quando <c>IDENT_MF</c> do Registro 0000 = "S", campos adicionais obrigatórios em moeda
/// funcional devem ser declarados neste registro conforme <c>REGRA_CAMPOS_ADICIONAIS_OBRIGATORIOS</c>.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 110–113.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_CAMPOS_ADICIONAIS</c>: campos adicionais não são validados pelo PGE, mas
///   sua existência é permitida.</item>
///   <item><c>REGRA_REG_COD_NUM_AD_DUPLICADO</c>: a chave REG_COD + NUM_AD não pode ser
///   duplicada; se violada, o PGE do Sped Contábil gera um erro.</item>
///   <item><c>REGRA_CAMPOS_ADICIONAIS_OBRIGATORIOS</c>: quando IDENT_MF (campo 19 do
///   Registro 0000) = "S", campos adicionais em moeda funcional listados no manual devem ser
///   definidos neste registro, conforme a forma de escrituração (I010.IND_ESC).</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I020", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I020";

    /// <summary>
    /// Código do registro que recepciona o campo adicional; valores válidos de "I050" a "I355".
    /// Campo <c>REG_COD</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? RegCod { get; set; }

    /// <summary>
    /// Número sequencial do campo adicional dentro do registro receptor.
    /// Campo <c>NUM_AD</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public int NumAd { get; set; }

    /// <summary>
    /// Nome do campo adicional.
    /// Campo <c>CAMPO</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? Campo { get; set; }

    /// <summary>
    /// Descrição do campo adicional.
    /// Campo <c>DESCRIÇÃO</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Indicação do tipo de dado: N = numérico (moeda, 2 decimais); C = caractere.
    /// Campo <c>TIPO</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public TipoDadoCampoAdicional Tipo { get; set; }
}
