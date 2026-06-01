using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0001 — Abertura do Bloco 0. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco 0. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 76.
/// </summary>
[RegistroSped(Codigo = "0001", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0001 : RegistroSped
{
    /// <summary>
    /// Indicador de movimento do Bloco 0: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    /// <inheritdoc />
    public override string Codigo => "0001";

    /// <summary>
    /// Indicador de movimento do Bloco 0: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
