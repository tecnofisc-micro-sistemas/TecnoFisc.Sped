using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F600 — Contribuição Retida na Fonte.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 277.
/// </summary>
[RegistroSped(Codigo = "F600", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF600 : RegistroSped
{
    public override string Codigo => "F600";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorNaturezaRetencao IndNatRet { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly DtRet { get; set; }

    /// <summary>Base de cálculo da retenção ou do recolhimento (sociedade cooperativa).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 4, Obrigatorio = true)]
    public decimal VlBcRet { get; set; }

    /// <summary>Valor total retido na fonte / recolhido (sociedade cooperativa).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRet { get; set; }

    /// <summary>Código da receita referente à retenção ou ao recolhimento.</summary>
    [CampoSped(Ordem = 6, Tamanho = 4)]
    public string? CodRec { get; set; }

    /// <summary>Indicador da natureza da receita (0=Não Cumulativa; 1=Cumulativa).</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorNaturezaCumulativa? IndNatRec { get; set; }

    /// <summary>CNPJ da Fonte Pagadora Responsável pela Retenção / Recolhimento ou da PJ Beneficiária.</summary>
    [CampoSped(Ordem = 8, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Valor retido na fonte – parcela referente ao PIS/Pasep.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetPis { get; set; }

    /// <summary>Valor retido na fonte – parcela referente à Cofins.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetCofins { get; set; }

    /// <summary>Indicador da condição da pessoa jurídica declarante (0=Beneficiária; 1=Responsável).</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDeclarante IndDec { get; set; }
}
