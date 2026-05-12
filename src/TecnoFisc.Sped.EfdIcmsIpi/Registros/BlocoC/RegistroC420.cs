using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C420 — Registro dos Totalizadores Parciais da Redução Z (código 02, 2D e 60).
/// Discrimina os valores por código de totalizador da Redução Z.
/// Nível hierárquico 4, ocorrência 1:N por C405.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 120.
/// </summary>
[RegistroSped(Codigo = "C420", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC420 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C420";

    /// <summary>Código do totalizador, conforme Tabela 4.4.6.</summary>
    [CampoSped(Ordem = 2, Tamanho = 7, Obrigatorio = true)]
    public string? CodTotPar { get; set; }

    /// <summary>Valor acumulado no totalizador, relativo à respectiva Redução Z.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlrAcumTot { get; set; }

    /// <summary>Número do totalizador quando ocorrer mais de uma situação com a mesma carga tributária efetiva.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public int? NrTot { get; set; }

    /// <summary>Descrição da situação tributária relativa ao totalizador parcial, quando houver mais de um com a mesma carga tributária efetiva.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? DescrNrTot { get; set; }
}
