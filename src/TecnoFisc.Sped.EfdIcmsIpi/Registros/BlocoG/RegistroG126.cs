using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G126 - Outros creditos CIAP.
/// Nivel hierarquico 4, ocorrencia varios por Registro G125. Conforme Guia Pratico
/// EFD-ICMS/IPI V3.0.6, p. 241-242.
/// </summary>
[RegistroSped(Codigo = "G126", Nivel = 4, Bloco = "G")]
public sealed partial class RegistroG126 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G126";

    /// <summary>Data inicial do periodo de apuracao (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do periodo de apuracao (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFim { get; set; }

    /// <summary>Numero da parcela do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public int NumParc { get; set; }

    /// <summary>Valor da parcela de ICMS passivel de apropriacao.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlParcPass { get; set; }

    /// <summary>Valor do somatorio das saidas tributadas e saidas para exportacao.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTribOc { get; set; }

    /// <summary>Valor total de saidas no periodo indicado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotal { get; set; }

    /// <summary>Indice de participacao das saidas tributadas e para exportacao no valor total de saidas.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 8, Obrigatorio = true)]
    public decimal IndPerSai { get; set; }

    /// <summary>Valor de outros creditos de ICMS a ser apropriado na apuracao.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlParcAprop { get; set; }
}
