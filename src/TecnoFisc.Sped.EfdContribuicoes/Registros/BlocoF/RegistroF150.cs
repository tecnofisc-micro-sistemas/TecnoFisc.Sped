using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F150 — Crédito Presumido sobre Estoque de Abertura. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 247.
/// </summary>
[RegistroSped(Codigo = "F150", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF150 : RegistroSped
{
    public override string Codigo => "F150";

    /// <summary>Código da Base de Cálculo do Crédito sobre Estoque de Abertura, conforme Tabela 4.3.7. Valor fixo: 18.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NatBcCred { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotEst { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? EstImp { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcEst { get; set; }

    /// <summary>Base de Cálculo Mensal do Crédito sobre o Estoque de Abertura (1/12 avos do campo VL_BC_EST).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcMenEst { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqPis { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPis { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofins { get; set; }

    /// <summary>Descrição do estoque.</summary>
    [CampoSped(Ordem = 13, Tamanho = 100)]
    public string? DescEst { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 14, Tamanho = 255)]
    public string? CodCta { get; set; }
}
