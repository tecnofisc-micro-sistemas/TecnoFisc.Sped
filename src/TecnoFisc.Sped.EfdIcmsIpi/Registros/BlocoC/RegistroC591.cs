using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C591 — Informações do Fundo de Combate à Pobreza (FCP) na NF3e (cód. 66).
/// Detalha o FCP vinculado à operação própria e à substituição tributária, agrupados
/// pela mesma combinação de CST_ICMS, CFOP e alíquota do ICMS do registro pai C590.
/// Nível hierárquico 4, ocorrência 1:1 (por registro C590).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 141.
/// </summary>
[RegistroSped(Codigo = "C591", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC591 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C591";

    /// <summary>Valor do FCP vinculado à operação própria, na combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlFcpOp { get; set; }

    /// <summary>Valor do FCP vinculado à operação de substituição tributária, na combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlFcpSt { get; set; }
}
