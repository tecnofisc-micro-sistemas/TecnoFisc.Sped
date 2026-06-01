using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C860 — Identificação do Equipamento SAT-CF-e.
/// Identifica os equipamentos SAT-CF-e utilizados na emissão de documentos fiscais.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 158.
/// </summary>
[RegistroSped(Codigo = "C860", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC860 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C860";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valor válido: 59 (CF-e-SAT).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Número de Série do equipamento SAT.</summary>
    [CampoSped(Ordem = 3, Tamanho = 9, Obrigatorio = true)]
    public int NrSat { get; set; }

    /// <summary>Data de emissão dos documentos fiscais, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Número do documento inicial (primeiro CF-e-SAT emitido no período pelo equipamento).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int DocIni { get; set; }

    /// <summary>Número do documento final (último CF-e-SAT emitido no período pelo equipamento).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public int DocFim { get; set; }
}
