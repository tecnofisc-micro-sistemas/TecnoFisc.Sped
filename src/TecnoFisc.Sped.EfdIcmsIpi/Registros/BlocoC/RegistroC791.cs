using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C791 — Registro de Informações de ICMS ST por UF (cód. 06).
/// Detalha, por unidade da federação, o ICMS ST retido referente ao C790.
/// Filho do C790. Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 150.
/// </summary>
[RegistroSped(Codigo = "C791", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC791 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C791";

    /// <summary>Sigla da unidade da federação a que se refere a retenção ST.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Valor da base de cálculo do ICMS substituição tributária.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcmsSt { get; set; }

    /// <summary>Valor do ICMS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsSt { get; set; }
}
