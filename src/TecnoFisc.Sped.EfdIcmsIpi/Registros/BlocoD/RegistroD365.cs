using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D365 — Registro dos Totalizadores Parciais da Redução Z
/// (cód. 2E, 13, 14, 15 e 16).
/// Nível hierárquico 4, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 185.
/// </summary>
[RegistroSped(Codigo = "D365", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD365 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D365";

    /// <summary>Código do totalizador, conforme Tabela 4.4.6.</summary>
    [CampoSped(Ordem = 2, Tamanho = 7, Obrigatorio = true)]
    public string? CodTotPar { get; set; }

    /// <summary>Valor acumulado no totalizador, relativo à respectiva Redução Z.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlrAcumTot { get; set; }

    /// <summary>Número do totalizador quando ocorrer mais de uma situação com a mesma carga tributária efetiva.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public int? NrTot { get; set; }

    /// <summary>Descrição da situação tributária relativa ao totalizador parcial.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? DescrNrTot { get; set; }
}
