using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C490 — Consolidação de Documentos Emitidos por ECF (Códigos 02, 2D, 59 e 60).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 160.
/// </summary>
[RegistroSped(Codigo = "C490", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC490 : RegistroSped
{
    public override string Codigo => "C490";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocIni { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocFin { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }
}
