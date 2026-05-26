using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco9;

/// <summary>
/// Registro 9001 — Abertura do Bloco 9. Nível hierárquico 1, ocorrência única por arquivo.
/// Indica se há ou não dados informados no Bloco 9 (Controle e Encerramento do Arquivo Digital).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 230.
/// </summary>
[RegistroSped(Codigo = "9001", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9001";

    /// <summary>
    /// Indicador de movimento do Bloco 9: 0 = bloco com dados informados; 1 = bloco sem dados
    /// informados. Campo <c>IND_DAD</c> no leiaute ECD.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndDad { get; set; }
}
