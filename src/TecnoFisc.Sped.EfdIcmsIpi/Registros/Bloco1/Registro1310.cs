using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1310 - Movimentacao Diaria de Combustiveis por Tanque. Nivel hierarquico 3,
/// ocorrencia varios por Registro 1300. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 276-277.
/// </summary>
/// <remarks>
/// <b>V020 (Guide 3.1.9 item 1; vigência fiscal V020 conforme Subseção 15):</b> adicionado o
/// campo 11 <c>CAP_TANQUE</c> (capacidade do tanque em litros, inteiro, ocorrência condicional).
/// </remarks>
[RegistroSped(Codigo = "1310", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1310";

    /// <summary>Tanque que armazena o combustivel.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? NumTanque { get; set; }

    /// <summary>Estoque no inicio do dia, em litros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal EstqAbert { get; set; }

    /// <summary>Volume recebido no dia, em litros.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolEntr { get; set; }

    /// <summary>Volume disponivel no dia, em litros.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolDisp { get; set; }

    /// <summary>Volume total das saidas, em litros.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolSaidas { get; set; }

    /// <summary>Estoque escritural, em litros.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal EstqEscr { get; set; }

    /// <summary>Valor da perda, em litros.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValAjPerda { get; set; }

    /// <summary>Valor do ganho, em litros.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValAjGanho { get; set; }

    /// <summary>Volume aferido no tanque, em litros; estoque de fechamento fisico do tanque.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal FechFisico { get; set; }

    /// <summary>
    /// Capacidade do tanque em litros. Introduzido em V020 (Guia Prático 3.1.9 item 1;
    /// vigência fiscal V020 conforme Subseção 15).
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 0, DesdeVersao = (int)LayoutEfdIcmsIpi.V020)]
    public int? CapTanque { get; set; }
}
