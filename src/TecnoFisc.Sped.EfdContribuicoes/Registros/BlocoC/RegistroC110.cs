using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C110 — Complemento do Documento - Informação Complementar da Nota Fiscal (Códigos 01, 1B, 04 e 55).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 114.
/// </summary>
[RegistroSped(Codigo = "C110", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC110 : RegistroSped
{
    public override string Codigo => "C110";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtComp { get; set; }
}
