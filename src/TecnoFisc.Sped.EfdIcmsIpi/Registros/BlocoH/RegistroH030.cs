using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

/// <summary>
/// Registro H030 — Informações Complementares do Inventário de Mercadorias Sujeitas ao Regime de ST.
/// Nível hierárquico 4, ocorrência 1:1 por registro H010. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 248.
/// </summary>
[RegistroSped(Codigo = "H030", Nivel = 4, Bloco = "H")]
public sealed partial class RegistroH030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "H030";

    /// <summary>Valor médio unitário do ICMS OP.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlIcmsOp { get; set; }

    /// <summary>Valor médio unitário da base de cálculo do ICMS ST.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor médio unitário do ICMS ST.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Valor médio unitário do FCP.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal? VlFcp { get; set; }
}
