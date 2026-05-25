using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C001 — Abertura do Bloco C. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco C. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 89.
/// </summary>
[RegistroSped(Codigo = "C001", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C001";

    /// <summary>
    /// Indicador de movimento do Bloco C: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD (semanticamente equivalente ao
    /// <c>IND_MOV</c> dos demais leiautes SPED).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
