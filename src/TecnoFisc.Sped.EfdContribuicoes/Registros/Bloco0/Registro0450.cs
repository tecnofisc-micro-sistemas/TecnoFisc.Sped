using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0450 — Tabela de Informação Complementar do Documento Fiscal. Nível hierárquico 3, ocorrência 1:N.
/// Filho do Registro 0001. Conforme Guia Prático v1.35, p. 88.
/// </summary>
[RegistroSped(Codigo = "0450", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0450 : RegistroSped
{
    public override string Codigo => "0450";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Txt { get; set; }
}
