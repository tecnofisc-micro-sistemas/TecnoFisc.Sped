using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I015 — Identificação das Contas da Escrituração Resumida a que se Refere a
/// Escrituração Auxiliar. Nível hierárquico 4, ocorrência 0:N por livro auxiliar (I012).
/// Preenchimento obrigatório quando <c>IND_ESC</c> do <see cref="RegistroI010"/> = "R", "A" ou "Z".
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 108–109.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_VALIDA_CONTA_RESUMIDA_AUXILIAR</c>: quando IND_ESC = "R" ou "B",
///   COD_CTA_RES deve corresponder a uma conta analítica/grupo de contas no plano de contas
///   do livro principal (I050), campo COD_CTA (campo 06). Se não cumprida, o PGE gera erro.</item>
///   <item><c>REGRA_VALIDA_CONTA_AUXILIAR_RESUMIDA</c>: quando IND_ESC = "A" ou "Z",
///   COD_CTA_RES deve corresponder a uma conta analítica/grupo de contas no plano de contas
///   do livro auxiliar (I050), campo COD_CTA (campo 06), com IND_CTA (campo 04) = "S"
///   (sintética). Se não cumprida, o PGE gera aviso.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I015", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI015 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I015";

    /// <summary>
    /// Código da(s) conta(s) analítica(s) do Livro Diário com Escrituração Resumida (R) que
    /// recebe os lançamentos globais; deve corresponder a uma conta sintética no livro auxiliar.
    /// Campo <c>COD_CTA_RES</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRes { get; set; }
}
