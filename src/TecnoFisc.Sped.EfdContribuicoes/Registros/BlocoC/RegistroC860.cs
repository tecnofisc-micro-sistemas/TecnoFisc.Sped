using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C860 — Identificação do Equipamento SAT-CF-e.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 184.
/// </summary>
[RegistroSped(Codigo = "C860", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC860 : RegistroSped
{
    public override string Codigo => "C860";

    /// <summary>Código do modelo do documento fiscal — Tabela 4.1.1. Valor válido: 59 (CF-e-SAT).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 9, Obrigatorio = true)]
    public int NrSat { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 9)]
    public int? DocIni { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 9)]
    public int? DocFim { get; set; }
}
