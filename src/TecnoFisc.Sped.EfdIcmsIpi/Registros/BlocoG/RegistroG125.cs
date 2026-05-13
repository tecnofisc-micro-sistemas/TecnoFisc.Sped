using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G125 — Movimentação de bem ou componente do ativo imobilizado.
/// Nível hierárquico 3, ocorrência vários por período de apuração. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 238-240.
/// </summary>
[RegistroSped(Codigo = "G125", Nivel = 3, Bloco = "G")]
public sealed partial class RegistroG125 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G125";

    /// <summary>Código individualizado do bem ou componente no controle patrimonial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodIndBem { get; set; }

    /// <summary>Data da movimentação ou do saldo inicial (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtMov { get; set; }

    /// <summary>Tipo de movimentação do bem ou componente.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public TipoMovimentacaoBemCiAp TipoMov { get; set; }

    /// <summary>Valor do ICMS da operação própria na entrada do bem ou componente.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlImobIcmsOp { get; set; }

    /// <summary>Valor do ICMS por substituição tributária na entrada do bem ou componente.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlImobIcmsSt { get; set; }

    /// <summary>Valor do ICMS sobre frete na entrada do bem ou componente.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlImobIcmsFrt { get; set; }

    /// <summary>Valor do ICMS diferencial de alíquota na entrada do bem ou componente.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlImobIcmsDif { get; set; }

    /// <summary>Número da parcela do ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 3)]
    public int? NumParc { get; set; }

    /// <summary>Valor da parcela de ICMS passível de apropriação.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlParcPass { get; set; }
}
