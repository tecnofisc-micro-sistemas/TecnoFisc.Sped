using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B460 — Deduções do ISS.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 54.
/// </summary>
[RegistroSped(Codigo = "B460", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB460 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B460";

    /// <summary>Indicador do tipo de dedução.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDeducaoIss IndDed { get; set; }

    /// <summary>Valor da dedução.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDed { get; set; }

    /// <summary>Número do processo ao qual o ajuste está vinculado, se houver.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorOrigemProcessoIss? IndProc { get; set; }

    /// <summary>Descrição do processo que embasou o lançamento.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? Proc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 7, Tamanho = 60, Obrigatorio = true)]
    public string? CodInfObs { get; set; }

    /// <summary>Indicador da obrigação onde será aplicada a dedução.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorObrigacaoIss IndObr { get; set; }
}
