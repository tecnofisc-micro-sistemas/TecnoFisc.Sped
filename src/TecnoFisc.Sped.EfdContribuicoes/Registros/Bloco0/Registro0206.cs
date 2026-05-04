using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0206 — Código de Produto Conforme Tabela ANP (Combustíveis). Nível hierárquico 4, ocorrência 1:1.
/// Filho do Registro 0200. Conforme Guia Prático v1.35, p. 86.
/// </summary>
[RegistroSped(Codigo = "0206", Nivel = 4, Bloco = "0")]
public sealed partial class Registro0206 : RegistroSped
{
    public override string Codigo => "0206";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodComb { get; set; }
}
