using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0208 — Código de Grupos por Marca Comercial – Refri (bebidas frias). Nível hierárquico 4, ocorrência 1:1.
/// Filho do Registro 0200. Aplicável apenas para fatos geradores até 30/04/2015. Conforme Guia Prático v1.35, p. 86-87.
/// </summary>
[RegistroSped(Codigo = "0208", Nivel = 4, Bloco = "0")]
public sealed partial class Registro0208 : RegistroSped
{
    public override string Codigo => "0208";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodTab { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodGru { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? MarcaCom { get; set; }
}
