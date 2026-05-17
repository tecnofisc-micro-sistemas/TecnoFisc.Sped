using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E316 — Obrigações recolhidas ou a recolher — Fundo de Combate à Pobreza e
/// ICMS Diferencial de Alíquota UF Origem/Destino EC 87/15.
/// Nível hierárquico 4, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 230-231.
/// </summary>
[RegistroSped(Codigo = "E316", Nivel = 4, Bloco = "E")]
public sealed partial class RegistroE316 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E316";

    /// <summary>Código da obrigação recolhida ou a recolher, conforme Tabela 5.4.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodOr { get; set; }

    /// <summary>Valor da obrigação recolhida ou a recolher.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOr { get; set; }

    /// <summary>Data de vencimento da obrigação no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtVcto { get; set; }

    /// <summary>Código de receita referente à obrigação, próprio da unidade da federação.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? CodRec { get; set; }

    /// <summary>Número do processo ou auto de infração ao qual a obrigação está vinculada, se houver.</summary>
    [CampoSped(Ordem = 6, Tamanho = 15)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorOrigemProcesso? IndProc { get; set; }

    /// <summary>Descrição resumida do processo que embasou o lançamento.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public string? Proc { get; set; }

    /// <summary>Descrição complementar das obrigações recolhidas ou a recolher.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? TxtCompl { get; set; }

    /// <summary>Mês de referência no formato mmaaaa.</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Obrigatorio = true)]
    public string? MesRef { get; set; }
}
