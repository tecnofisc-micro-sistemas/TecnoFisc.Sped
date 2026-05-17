using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0500 — Plano de Contas Contábeis. Nível hierárquico 2, ocorrência vários por arquivo.
/// Identifica as contas contábeis utilizadas pelo contribuinte em sua Contabilidade Geral,
/// relativas às contas referenciadas no Registro 0300. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 42.
/// </summary>
[RegistroSped(Codigo = "0500", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0500";

    /// <summary>Data da inclusão/alteração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>Código da natureza da conta/grupo de contas.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoNaturezaContaContabil CodNatCc { get; set; }

    /// <summary>Indicador do tipo de conta: S — Sintética (grupo de contas); A — Analítica (conta).</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public string? IndCta { get; set; }

    /// <summary>Nível da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 5, Obrigatorio = true)]
    public int Nivel { get; set; }

    /// <summary>Código da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 6, Tamanho = 60, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Nome da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 7, Tamanho = 60, Obrigatorio = true)]
    public string? NomeCta { get; set; }
}
