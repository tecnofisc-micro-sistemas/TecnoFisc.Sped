using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I001 — Abertura do Bloco I. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco I. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 102.
/// </summary>
[RegistroSped(Codigo = "I001", Nivel = 1, Bloco = "I")]
public sealed partial class RegistroI001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I001";

    /// <summary>
    /// Indicador de movimento do Bloco I: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
