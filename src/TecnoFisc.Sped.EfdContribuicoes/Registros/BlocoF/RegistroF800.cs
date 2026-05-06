using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F800 — Créditos Decorrentes de Eventos de Incorporação, Fusão e Cisão.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 282.
/// </summary>
[RegistroSped(Codigo = "F800", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF800 : RegistroSped
{
    public override string Codigo => "F800";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorNaturezaEvento IndNatEven { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly DtEven { get; set; }

    /// <summary>CNPJ da pessoa jurídica sucedida que apurou o crédito.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14, Obrigatorio = true)]
    public Cnpj CnpjSuced { get; set; }

    /// <summary>Período de apuração do crédito pela PJ sucedida (MMAAAA).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? PaContCred { get; set; }

    /// <summary>Código do crédito transferido, conforme Tabela 4.3.6.</summary>
    [CampoSped(Ordem = 6, Tamanho = 3, Obrigatorio = true)]
    public CodigoTipoCredito CodCred { get; set; }

    /// <summary>Valor do crédito de PIS/Pasep adquirido em decorrência do evento de sucessão.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPis { get; set; }

    /// <summary>Valor do crédito de Cofins adquirido em decorrência do evento de sucessão.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofins { get; set; }

    /// <summary>Percentual do crédito original transferido, no caso de evento de Cisão.</summary>
    [CampoSped(Ordem = 9, Tamanho = 6, Decimais = 2)]
    public decimal? PerCredCis { get; set; }
}
