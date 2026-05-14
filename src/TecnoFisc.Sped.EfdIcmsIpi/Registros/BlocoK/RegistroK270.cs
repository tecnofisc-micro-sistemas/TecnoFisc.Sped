using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K270 — Correção de Apontamento dos Registros K210, K220, K230, K250,
/// K260, K291, K292, K301 e K302. Nível hierárquico 3, ocorrência vários por
/// registro K100. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 259-261.
/// </summary>
[RegistroSped(Codigo = "K270", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK270 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K270";

    /// <summary>Data inicial do período de apuração do apontamento corrigido (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniAp { get; set; }

    /// <summary>Data final do período de apuração do apontamento corrigido (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinAp { get; set; }

    /// <summary>Código de identificação da ordem de produção ou da ordem de serviço corrigida.</summary>
    [CampoSped(Ordem = 4, Tamanho = 30)]
    public string? CodOpOs { get; set; }

    /// <summary>Código da mercadoria que está sendo corrigido (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade de correção positiva de apontamento de período anterior.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6)]
    public decimal? QtdCorPos { get; set; }

    /// <summary>Quantidade de correção negativa de apontamento de período anterior.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6)]
    public decimal? QtdCorNeg { get; set; }

    /// <summary>Origem da correção de apontamento.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemCorrecaoApontamento Origem { get; set; }
}
