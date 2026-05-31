using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1300 - Movimentacao Diaria de Combustiveis. Nivel hierarquico 2,
/// ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 275-276.
/// </summary>
[RegistroSped(Codigo = "1300", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1300";

    /// <summary>Codigo do produto, constante do registro 0200.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Data do fechamento da movimentacao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFech { get; set; }

    /// <summary>Estoque no inicio do dia, em litros.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal EstqAbert { get; set; }

    /// <summary>Volume recebido no dia, em litros.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolEntr { get; set; }

    /// <summary>Volume disponivel no dia, em litros.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolDisp { get; set; }

    /// <summary>Volume total das saidas, em litros.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolSaidas { get; set; }

    /// <summary>Estoque escritural, em litros.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal EstqEscr { get; set; }

    /// <summary>Valor da perda, em litros.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValAjPerda { get; set; }

    /// <summary>Valor do ganho, em litros.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValAjGanho { get; set; }

    /// <summary>Estoque de fechamento, em litros.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal FechFisico { get; set; }
}
