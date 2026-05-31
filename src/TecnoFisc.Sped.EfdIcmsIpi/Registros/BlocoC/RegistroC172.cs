using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C172 — Operações com ISSQN (cód. 01).
/// Nível hierárquico 4, ocorrência 1:1 (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 82.
/// </summary>
[RegistroSped(Codigo = "C172", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC172 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C172";

    /// <summary>Valor da base de cálculo do ISSQN.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIssqn { get; set; }

    /// <summary>Alíquota do ISSQN.</summary>
    [CampoSped(Ordem = 3, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIssqn { get; set; }

    /// <summary>Valor do ISSQN.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlIssqn { get; set; }
}
