using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Registros.Bloco0;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C051 — Plano de Contas Referencial Recuperado. Nível hierárquico 4, ocorrência 0:N.
/// Identifica o plano de contas referencial referente ao arquivo da ECD recuperado (Registro I051).
/// O código do plano referencial (<c>COD_CTA_REF</c>) segue a tabela publicada pelos órgãos
/// indicados no campo <c>COD_PLAN_REF</c> do <see cref="Registro0000"/>.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 94.
/// </summary>
[RegistroSped(Codigo = "C051", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC051 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C051";

    /// <summary>Código do centro de custo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? CodCcus { get; set; }

    /// <summary>
    /// Código da conta de acordo com o plano de contas referencial, conforme tabela publicada
    /// pelos órgãos indicados no campo <c>COD_PLAN_REF</c> do <see cref="Registro0000"/>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRef { get; set; }
}
