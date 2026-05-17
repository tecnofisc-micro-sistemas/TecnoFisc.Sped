using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E116 — Obrigações do ICMS Recolhido ou a Recolher — Operações Próprias.
/// Nível hierárquico 4, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 212.
/// </summary>
[RegistroSped(Codigo = "E116", Nivel = 4, Bloco = "E")]
public sealed partial class RegistroE116 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E116";

    /// <summary>Código da obrigação a recolher, conforme Tabela 5.4 do guia.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodOr { get; set; }

    /// <summary>Valor da obrigação a recolher.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOr { get; set; }

    /// <summary>Data de vencimento da obrigação no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtVcto { get; set; }

    /// <summary>Código de receita referente à obrigação, próprio da unidade da federação, conforme legislação estadual.</summary>
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

    /// <summary>Descrição complementar das obrigações a recolher.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? TxtCompl { get; set; }

    /// <summary>Mês de referência no formato mmaaaa (obrigatório a partir de jan/2011).</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Obrigatorio = true)]
    public string? MesRef { get; set; }
}
