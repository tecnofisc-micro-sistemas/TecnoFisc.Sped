using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P200 — Consolidação da Contribuição Previdenciária Sobre a Receita Bruta.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Somatório da CPRB apurada nos
/// Registros P100 do período. Conforme Guia Prático v1.35, p. 371.
/// </summary>
[RegistroSped(Codigo = "P200", Nivel = 2, Bloco = "P")]
public sealed partial class RegistroP200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P200";

    /// <summary>Período de referência da escrituração no formato MMAAAA (6 caracteres fixos).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? PerRef { get; set; }

    /// <summary>Valor total apurado da CPRB no período (somatório do campo VL_CONT_APU dos Registros P100).</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContApu { get; set; }

    /// <summary>Valor total de ajustes de redução (Registro P210, Campo VL_AJ, quando IND_AJ = 0).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlTotAjReduc { get; set; }

    /// <summary>Valor total de ajustes de acréscimo (Registro P210, Campo VL_AJ, quando IND_AJ = 1).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlTotAjAcres { get; set; }

    /// <summary>
    /// Valor total da CPRB a recolher no período
    /// (Campo VL_TOT_CONT_APU – Campo VL_TOT_AJ_REDUC + Campo VL_TOT_AJ_ACRES).
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContDev { get; set; }

    /// <summary>
    /// Código de Receita referente à CPRB, conforme informado em DCTF (6 caracteres fixos, sem hífen).
    /// Exemplos: 298501, 298504, 298506, 299101.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 6, Obrigatorio = true)]
    public string? CodRec { get; set; }
}
