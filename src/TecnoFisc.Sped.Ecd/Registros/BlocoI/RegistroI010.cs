using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I010 — Identificação da Escrituração Contábil. Nível hierárquico 2, ocorrência
/// única por arquivo. Conforme Manual de Orientação do Leiaute 9 da ECD, p. 102–104.
/// </summary>
/// <remarks>
/// O campo <c>IND_ESC</c> (campo 02) dirige a obrigatoriedade condicional de vários registros
/// filhos (I012, I015, etc.) e de regras cruzadas ao longo do Bloco I. Por
/// <c>ARCHITECTURE.md §2.3</c>, essa responsabilidade é do consumidor: o pacote apenas
/// deserializa o valor; não há validador interno condicionado a <see cref="IndEsc"/>.
/// </remarks>
[RegistroSped(Codigo = "I010", Nivel = 2, Bloco = "I")]
public sealed partial class RegistroI010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I010";

    /// <summary>
    /// Indicador da forma de escrituração contábil. Valores: G = Livro Diário (Completo sem
    /// escrituração auxiliar); R = Livro Diário com Escrituração Resumida; A = Livro Diário
    /// Auxiliar ao Diário com Escrituração Resumida; B = Livro Balancetes Diários e Balanços;
    /// Z = Razão Auxiliar (conforme leiaute dos registros I500–I555).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public FormaEscrituracaoContabil IndEsc { get; set; }

    /// <summary>
    /// Código da versão do leiaute contábil. A partir do ano-calendário 2020: <c>"9.00"</c>.
    /// Validado pela <c>REGRA_VERSAO_LC</c> do PGE do Sped Contábil.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodVerLc { get; set; }
}
