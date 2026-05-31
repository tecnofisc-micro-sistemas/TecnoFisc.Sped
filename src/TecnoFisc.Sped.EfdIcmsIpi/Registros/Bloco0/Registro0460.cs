using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0460 — Tabela de Observações do Lançamento Fiscal. Nível hierárquico 2,
/// ocorrência vários por arquivo. Codifica anotações de escrituração determinadas pela
/// legislação pertinente aos lançamentos fiscais (ajustes por diferimento, antecipações,
/// diferencial de alíquota etc.). Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 41.
/// </summary>
[RegistroSped(Codigo = "0460", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0460 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0460";

    /// <summary>Código da Observação do lançamento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodObs { get; set; }

    /// <summary>Descrição da observação vinculada ao lançamento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Txt { get; set; }
}
