using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D355 — Redução Z dos equipamentos ECF utilizados na emissão de Cupom Fiscal
/// Bilhete de Passagem (2E), Bilhete de Passagem Rodoviário (13), Aquaviário (14),
/// Passagem e Nota de Bagagem (15) e Ferroviário (16).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 184.
/// </summary>
[RegistroSped(Codigo = "D355", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D355";

    /// <summary>Data do movimento a que se refere a Redução Z, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Posição do Contador de Reinício de Operação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public int? Cro { get; set; }

    /// <summary>Posição do Contador de Redução Z.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public int? Crz { get; set; }

    /// <summary>Número do Contador de Ordem de Operação do último documento emitido no dia (COO na Redução Z).</summary>
    [CampoSped(Ordem = 5, Tamanho = 9, Obrigatorio = true)]
    public int? NumCooFin { get; set; }

    /// <summary>Valor do Grande Total final.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal GtFin { get; set; }

    /// <summary>Valor da venda bruta.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBrt { get; set; }
}
