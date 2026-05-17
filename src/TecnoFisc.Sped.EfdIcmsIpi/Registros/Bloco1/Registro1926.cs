using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1926 - Obrigacoes do ICMS a recolher - operacoes referentes a sub-apuracao.
/// Nivel hierarquico 5, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 295-296.
/// </summary>
[RegistroSped(Codigo = "1926", Nivel = 5, Bloco = "1")]
public sealed partial class Registro1926 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1926";

    /// <summary>Codigo da obrigacao a recolher, conforme a Tabela 5.4.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodOr { get; set; }

    /// <summary>Valor da obrigacao a recolher.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOr { get; set; }

    /// <summary>Data de vencimento da obrigacao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtVcto { get; set; }

    /// <summary>Codigo de receita referente a obrigacao, proprio da unidade da federacao.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? CodRec { get; set; }

    /// <summary>Numero do processo ou auto de infracao ao qual a obrigacao esta vinculada, se houver.</summary>
    [CampoSped(Ordem = 6, Tamanho = 15)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorOrigemProcesso? IndProc { get; set; }

    /// <summary>Descricao resumida do processo que embasou o lancamento.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public string? Proc { get; set; }

    /// <summary>Descricao complementar das obrigacoes a recolher.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? TxtCompl { get; set; }

    /// <summary>Mes de referencia no formato mmaaaa.</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Obrigatorio = true)]
    public string? MesRef { get; set; }
}
