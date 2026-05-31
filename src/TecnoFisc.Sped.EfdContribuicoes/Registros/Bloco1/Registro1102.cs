using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1102 — Detalhamento do Crédito Extemporâneo Vinculado a Mais de Um Tipo de Receita – PIS/Pasep.
/// Nível hierárquico 4, ocorrência 1:1. Filho do Registro 1101.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 390.
/// </summary>
[RegistroSped(Codigo = "1102", Nivel = 4, Bloco = "1")]
public sealed partial class Registro1102 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1102";

    /// <summary>Parcela do Crédito de PIS/PASEP vinculada à Receita Tributada no Mercado Interno.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPisTribMi { get; set; }

    /// <summary>Parcela do Crédito de PIS/PASEP vinculada à Receita Não Tributada no Mercado Interno.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPisNtMi { get; set; }

    /// <summary>Parcela do Crédito de PIS/PASEP vinculada à Receita de Exportação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredPisExp { get; set; }
}
