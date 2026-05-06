using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C400 — Equipamento ECF (Códigos 02 e 2D).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 154.
/// </summary>
[RegistroSped(Codigo = "C400", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC400 : RegistroSped
{
    public override string Codigo => "C400";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? EcfMod { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 21, Obrigatorio = true)]
    public string? EcfFab { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public int EcfCx { get; set; }
}
