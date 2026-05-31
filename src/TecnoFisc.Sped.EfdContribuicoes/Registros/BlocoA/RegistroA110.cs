using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A110 — Complemento do Documento - Informação Complementar da NF. Nível hierárquico 4,
/// ocorrência 1:N. Conforme Guia Prático v1.35, p. 98.
/// </summary>
[RegistroSped(Codigo = "A110", Nivel = 4, Bloco = "A")]
public sealed partial class RegistroA110 : RegistroSped
{
    public override string Codigo => "A110";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}
