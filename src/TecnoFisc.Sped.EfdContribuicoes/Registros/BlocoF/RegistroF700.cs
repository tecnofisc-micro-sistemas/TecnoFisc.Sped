using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F700 — Deduções Diversas.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 280.
/// </summary>
[RegistroSped(Codigo = "F700", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF700 : RegistroSped
{
    public override string Codigo => "F700";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorOrigemDeducoesDiversas IndOriDed { get; set; }

    /// <summary>Indicador da natureza da dedução (0=Não Cumulativa; 1=Cumulativa).</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorNaturezaCumulativa IndNatDed { get; set; }

    /// <summary>Valor a deduzir — PIS/PASEP.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDedPis { get; set; }

    /// <summary>Valor a deduzir — Cofins.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDedCofins { get; set; }

    /// <summary>Valor da base de cálculo da operação que ensejou o valor a deduzir.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcOper { get; set; }

    /// <summary>CNPJ da pessoa jurídica relacionada à operação.</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>Informações complementares do documento/operação.</summary>
    [CampoSped(Ordem = 8, Tamanho = 90)]
    public string? InfComp { get; set; }
}
