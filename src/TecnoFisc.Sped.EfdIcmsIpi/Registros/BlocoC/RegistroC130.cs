using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C130 — Complemento — ISSQN, IRRF e Previdência Social.
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 73.
/// </summary>
[RegistroSped(Codigo = "C130", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC130 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C130";

    /// <summary>Valor dos serviços sob não-incidência ou não-tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valor da base de cálculo do ISSQN.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIssqn { get; set; }

    /// <summary>Valor do ISSQN.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlIssqn { get; set; }

    /// <summary>Valor da base de cálculo do Imposto de Renda Retido na Fonte.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIrrf { get; set; }

    /// <summary>Valor do Imposto de Renda Retido na Fonte.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlIrrf { get; set; }

    /// <summary>Valor da base de cálculo de retenção da Previdência Social.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPrev { get; set; }

    /// <summary>Valor destacado para retenção da Previdência Social.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlPrev { get; set; }
}
