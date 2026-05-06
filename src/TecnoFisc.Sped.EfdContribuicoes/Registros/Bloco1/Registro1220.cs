using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1220 — Demonstração do Crédito a Descontar a Contribuição Extemporânea – PIS/Pasep.
/// Nível hierárquico 3, ocorrência 1:N. Filho do Registro 1200. Demonstra os créditos a descontar
/// da contribuição extemporânea, segregados por período de apuração, origem e código de crédito.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 393.
/// </summary>
[RegistroSped(Codigo = "1220", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1220 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1220";

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
