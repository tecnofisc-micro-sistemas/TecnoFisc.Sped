using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1620 — Demonstração do Crédito a Descontar da Contribuição Extemporânea – Cofins.
/// Nível hierárquico 3, ocorrência 1:N. Filho do Registro 1600. Demonstra os créditos a descontar
/// da contribuição extemporânea, segregados por período de apuração, origem e código de crédito.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 405.
/// </summary>
[RegistroSped(Codigo = "1620", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1620 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1620";

    /// <summary>Período de apuração do crédito descontado no formato MMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? PerApuCred { get; set; }

    /// <summary>Indicador da origem do crédito.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorOrigemCreditoExtemporaneo OrigCred { get; set; }

    /// <summary>Código do tipo do crédito, conforme Tabela 4.3.6.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public CodigoTipoCredito CodCred { get; set; }

    /// <summary>Valor do crédito a descontar.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCred { get; set; }
}
