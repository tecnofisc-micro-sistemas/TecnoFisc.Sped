using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I200 — Composição das Receitas, Deduções e/ou Exclusões do Período.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 289.
/// </summary>
[RegistroSped(Codigo = "I200", Nivel = 4, Bloco = "I")]
public sealed partial class RegistroI200 : RegistroSped
{
    public override string Codigo => "I200";

    /// <summary>Número do campo do registro I100 (02, 04 ou 05) objeto de demonstração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NumCampo { get; set; }

    /// <summary>Código do tipo de detalhamento conforme Tabelas 7.1.1 e/ou 7.1.2.</summary>
    [CampoSped(Ordem = 3, Tamanho = 5, Obrigatorio = true)]
    public string? CodDet { get; set; }

    /// <summary>Valor detalhado referente ao campo COD_DET deste registro.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? DetValor { get; set; }

    /// <summary>Código da conta contábil referente ao valor informado em DET_VALOR.</summary>
    [CampoSped(Ordem = 5, Tamanho = 255)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
