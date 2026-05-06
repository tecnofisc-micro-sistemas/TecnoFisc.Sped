using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M350 — PIS/Pasep – Folha de Salários.
/// Nível hierárquico 2, ocorrência um (por arquivo).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 325.
/// </summary>
[RegistroSped(Codigo = "M350", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM350 : RegistroSped
{
    public override string Codigo => "M350";

    /// <summary>Valor Total da Folha de Salários.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotFol { get; set; }

    /// <summary>Valor Total das Exclusões à Base de Cálculo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlExcBc { get; set; }

    /// <summary>Valor Total da Base de Cálculo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotBc { get; set; }

    /// <summary>Alíquota do PIS/PASEP – Folha de Salários (valor válido: 1%).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Decimais = 2, Obrigatorio = true)]
    public decimal AliqPisFol { get; set; }

    /// <summary>Valor Total da Contribuição Social sobre a Folha de Salários.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContFol { get; set; }
}
