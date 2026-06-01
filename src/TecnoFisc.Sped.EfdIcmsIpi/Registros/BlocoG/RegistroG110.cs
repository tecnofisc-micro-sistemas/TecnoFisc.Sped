using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G110 — ICMS – Ativo Permanente – CIAP.
/// Nível hierárquico 2, ocorrência um por período de apuração. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 237.
/// </summary>
[RegistroSped(Codigo = "G110", Nivel = 2, Bloco = "G")]
public sealed partial class RegistroG110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G110";

    /// <summary>Data inicial a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Saldo inicial de ICMS do CIAP.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SaldoInIcms { get; set; }

    /// <summary>Somatório das parcelas de ICMS passíveis de apropriação de cada bem.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SomParc { get; set; }

    /// <summary>Valor do somatório das saídas tributadas e saídas para exportação.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTribExp { get; set; }

    /// <summary>Valor total de saídas.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotal { get; set; }

    /// <summary>Índice de participação das saídas tributadas e para exportação no valor total de saídas.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 8, Obrigatorio = true)]
    public decimal IndPerSai { get; set; }

    /// <summary>Valor de ICMS a ser apropriado na apuração do ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal IcmsAprop { get; set; }

    /// <summary>Valor de outros créditos a ser apropriado na apuração do ICMS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SomIcmsOc { get; set; }
}
