using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I300 — Detalhamento das Receitas, Deduções e/ou Exclusões do Período.
/// Nível hierárquico 5, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 291.
/// </summary>
[RegistroSped(Codigo = "I300", Nivel = 5, Bloco = "I")]
public sealed partial class RegistroI300 : RegistroSped
{
    public override string Codigo => "I300";

    /// <summary>Código das Tabelas 7.1.3 ou 7.1.4 (visão analítica/referenciada), objeto de complemento.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodComp { get; set; }

    /// <summary>Valor da receita, dedução ou exclusão conforme código em COD_COMP.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? DetValor { get; set; }

    /// <summary>Código da conta contábil referente ao valor informado em DET_VALOR.</summary>
    [CampoSped(Ordem = 4, Tamanho = 255)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
