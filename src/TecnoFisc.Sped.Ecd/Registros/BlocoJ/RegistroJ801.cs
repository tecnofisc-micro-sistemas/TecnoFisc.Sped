using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J801 — Termo de Verificação para Fins de Substituição da ECD. Nível hierárquico 3,
/// ocorrência 0:1 por arquivo. Obrigatório quando a ECD for substituta (campo
/// <c>IND_FIN_ESC</c> do Registro 0000 igual a "1"). Limite de 30 MB para o arquivo RTF.
/// Conforme Instrução Normativa RFB nº 2.003/2021 e Manual de Orientação do Leiaute 9 da ECD, p. 192–195.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_NAO_DEVE_EXISTIR_NO_RTF</c>: o campo <see cref="ArqRtf"/> não pode
///   conter as tags C001, I001, J001, K001, J800, J801 ou J900; descumprimento gera erro.</item>
///   <item><c>REGRA_VALIDA_HASH_ARQUIVO</c>: <see cref="HashRtf"/> deve ser igual ao hash
///   calculado sobre o conteúdo de <see cref="ArqRtf"/>; descumprimento gera erro.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J801", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ801 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J801";

    /// <summary>
    /// Tipo de documento. Valor fixo "001" (Termo de Verificação para Fins de Substituição da ECD).
    /// Campo <c>TIPO_DOC</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? TipoDoc { get; set; }

    /// <summary>
    /// Descrição do arquivo .rtf.
    /// Campo <c>DESC_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescRtf { get; set; }

    /// <summary>
    /// Código do motivo preponderante da substituição da ECD.
    /// Campo <c>COD_MOT_SUBS</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 10, Obrigatorio = true)]
    public CodigoMotivoSubstituicaoEcd CodMotSubs { get; set; }

    /// <summary>
    /// Hash do arquivo .rtf incluído (41 caracteres). Preenchido automaticamente pelo
    /// sistema — não editável nem alterável pelo declarante.
    /// Campo <c>HASH_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 41)]
    public string? HashRtf { get; set; }

    /// <summary>
    /// Sequência de bytes que representam o arquivo no formato RTF (Rich Text Format),
    /// com limite de 30 MB. Serializado diretamente no campo SPED sem codificação adicional.
    /// Campo <c>ARQ_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? ArqRtf { get; set; }

    /// <summary>
    /// Indicador de fim do arquivo RTF. Texto fixo contendo "J801FIM".
    /// Campo <c>IND_FIM_RTF</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 7, Obrigatorio = true)]
    public string? IndFimRtf { get; set; }
}
