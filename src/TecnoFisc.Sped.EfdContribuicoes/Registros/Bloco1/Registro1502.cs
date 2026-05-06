using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1502 — Detalhamento do Crédito Extemporâneo Vinculado a Mais de Um Tipo de Receita – Cofins.
/// Nível hierárquico 4, ocorrência 1:1. Filho do Registro 1501.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 402.
/// </summary>
[RegistroSped(Codigo = "1502", Nivel = 4, Bloco = "1")]
public sealed partial class Registro1502 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1502";

    /// <summary>Parcela do Crédito de COFINS vinculada à Receita Tributada no Mercado Interno.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredCofinsTribMi { get; set; }

    /// <summary>Parcela do Crédito de COFINS vinculada à Receita Não Tributada no Mercado Interno.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredCofinsNtMi { get; set; }

    /// <summary>Parcela do Crédito de COFINS vinculada à Receita de Exportação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredCofinsExp { get; set; }
}
