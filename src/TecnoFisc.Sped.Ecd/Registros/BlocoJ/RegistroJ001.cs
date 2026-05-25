using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J001 — Abertura do Bloco J. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco J. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 170.
/// </summary>
[RegistroSped(Codigo = "J001", Nivel = 1, Bloco = "J")]
public sealed partial class RegistroJ001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J001";

    /// <summary>
    /// Indicador de movimento do Bloco J: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
