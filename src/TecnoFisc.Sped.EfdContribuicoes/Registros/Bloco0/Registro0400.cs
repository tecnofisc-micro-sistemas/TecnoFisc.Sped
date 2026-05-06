using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0400 — Tabela de Natureza da Operação/Prestação. Nível hierárquico 3, ocorrência 1:N.
/// Filho do Registro 0001. Conforme Guia Prático v1.35, p. 88.
/// </summary>
[RegistroSped(Codigo = "0400", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0400 : RegistroSped
{
    public override string Codigo => "0400";

    [CampoSped(Ordem = 2, Tamanho = 10, Obrigatorio = true)]
    public string? CodNat { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrNat { get; set; }
}
