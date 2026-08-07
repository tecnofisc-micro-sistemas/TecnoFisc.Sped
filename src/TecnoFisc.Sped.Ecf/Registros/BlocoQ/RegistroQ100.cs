using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoQ;

/// <summary>Registro Q100 - demonstrativo do Livro Caixa.</summary>
[RegistroSped(Codigo = "Q100", Nivel = 2, Bloco = "Q")]
public sealed partial class RegistroQ100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Q100";

    /// <summary>Data da entrada ou da saída dos recursos.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DATA")]
    public DateOnly Data { get; set; }

    /// <summary>Número do documento, preservado sem normalização.</summary>
    [CampoSped(Ordem = 3, Nome = "NUM_DOC")]
    public string? NumDoc { get; set; }

    /// <summary>Histórico do lançamento.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "HIST")]
    public string? Hist { get; set; }

    /// <summary>Valor de entrada dos recursos.</summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Nome = "VL_ENTRADA")]
    public decimal? VlEntrada { get; set; }

    /// <summary>Valor de saída dos recursos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Nome = "VL_SAIDA")]
    public decimal? VlSaida { get; set; }

    /// <summary>Saldo final declarado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SLD_FIN")]
    public decimal SldFin { get; set; }
}
