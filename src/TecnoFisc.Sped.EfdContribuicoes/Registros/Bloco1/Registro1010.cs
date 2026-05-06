using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1010 — Processo Referenciado – Ação Judicial.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Referencia decisões judiciais
/// que autorizam procedimento específico de apuração de PIS/Pasep e Cofins.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 375.
/// </summary>
[RegistroSped(Codigo = "1010", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1010";

    /// <summary>Número do processo judicial (até 20 caracteres).</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    /// <summary>Identificação da seção judiciária onde foi ajuizado o processo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? IdSecJud { get; set; }

    /// <summary>Identificação da vara da seção judiciária (até 2 caracteres).</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? IdVara { get; set; }

    /// <summary>
    /// Indicador da natureza da ação judicial, conforme tabela de códigos do Guia Prático v1.35, p. 375.
    /// Códigos 12–19 indicam ações com exigibilidade de contribuição suspensa.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public IndicadorNaturezaAcaoJudicial IndNatAcao { get; set; }

    /// <summary>Descrição resumida dos efeitos tributários abrangidos pela decisão judicial (até 100 caracteres).</summary>
    [CampoSped(Ordem = 6, Tamanho = 100)]
    public string? DescDecJud { get; set; }

    /// <summary>Data da sentença/decisão judicial no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtSentJud { get; set; }
}
