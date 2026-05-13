using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E520 — Apuração do IPI.
/// Nível hierárquico 3, ocorrência 1:1 por período. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 233.
/// </summary>
[RegistroSped(Codigo = "E520", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE520 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E520";

    /// <summary>Saldo credor do IPI transferido do período anterior.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSdAntIpi { get; set; }

    /// <summary>Valor total dos débitos por saídas com débito do imposto.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDebIpi { get; set; }

    /// <summary>Valor total dos créditos por entradas e aquisições com crédito do imposto.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredIpi { get; set; }

    /// <summary>Valor de outros débitos do IPI, inclusive estornos de crédito.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOdIpi { get; set; }

    /// <summary>Valor de outros créditos do IPI, inclusive estornos de débitos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOcIpi { get; set; }

    /// <summary>Valor do saldo credor do IPI a transportar para o período seguinte.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlScIpi { get; set; }

    /// <summary>Valor do saldo devedor do IPI a recolher.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSdIpi { get; set; }
}
