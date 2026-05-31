using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C050 — Plano de Contas Recuperado. Nível hierárquico 3, ocorrência 1:N por arquivo.
/// Identifica o plano de contas referente ao arquivo de ECD recuperado (Registro I050).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 93.
/// </summary>
[RegistroSped(Codigo = "C050", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC050 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C050";

    /// <summary>Data da inclusão/alteração da conta (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>Código da natureza da conta/grupo de contas, conforme tabela publicada pelo Sped.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodNat { get; set; }

    /// <summary>Indicador do tipo de conta: A = Analítica (conta), S = Sintética (grupo de contas).</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta IndCta { get; set; }

    /// <summary>Nível da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int Nivel { get; set; }

    /// <summary>Código da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Código da conta sintética/grupo de contas de nível imediatamente superior.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? CodCtaSup { get; set; }

    /// <summary>Nome da conta analítica/grupo de contas.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Obrigatorio = true)]
    public string? Cta { get; set; }
}
