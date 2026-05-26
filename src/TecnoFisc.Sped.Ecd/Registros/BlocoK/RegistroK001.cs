using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K001 — Abertura do Bloco K. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco K (Conglomerados Econômicos). Conforme Manual
/// de Orientação do Leiaute 9 da ECD, p. 209.
/// </summary>
[RegistroSped(Codigo = "K001", Nivel = 1, Bloco = "K")]
public sealed partial class RegistroK001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K001";

    /// <summary>
    /// Indicador de movimento do Bloco K: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
