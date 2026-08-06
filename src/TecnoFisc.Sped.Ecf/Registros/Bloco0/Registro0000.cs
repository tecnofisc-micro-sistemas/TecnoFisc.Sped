using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>
/// Registro 0000 — abertura do arquivo digital e identificação da pessoa jurídica.
/// Leiaute 12, páginas 58–67 do manual da ECF.
/// </summary>
[RegistroSped(Codigo = "0000", Nivel = 0, Bloco = "0")]
public sealed partial class Registro0000 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0000";

    /// <inheritdoc />
    public override int VersaoLeiaute => CodVer switch
    {
        "0008" => 8,
        "0009" => 9,
        "0010" => 10,
        "0011" => 11,
        "0012" => 12,
        _ => 0,
    };

    /// <summary>Identificador fixo do tipo de escrituração: <c>LECF</c>.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? NomeEsc { get; set; } = "LECF";

    /// <summary>Código declarado da versão do leiaute, de <c>0008</c> a <c>0012</c>.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public string? CodVer { get; set; }

    /// <summary>CNPJ do declarante ou do sócio ostensivo, no caso de SCP.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Nome empresarial da pessoa jurídica ou da SCP.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>Indicador do início do período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public int IndSitIniPer { get; set; }

    /// <summary>Indicador de situação especial ou outro evento.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public string? SitEspecial { get; set; }

    /// <summary>Percentual do patrimônio remanescente em caso de cisão.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4)]
    public decimal? PatRemanCis { get; set; }

    /// <summary>Data da situação especial ou do evento.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtSitEsp { get; set; }

    /// <summary>Data inicial das informações contidas no arquivo.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das informações contidas no arquivo.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Indicador de escrituração original, retificadora ou com mudança de tributação.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public string? Retificadora { get; set; }

    /// <summary>Número do recibo da ECF anterior, quando aplicável.</summary>
    [CampoSped(Ordem = 13, Tamanho = 40)]
    public string? NumRec { get; set; }

    /// <summary>Indicador do tipo da ECF.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public int TipEcf { get; set; }

    /// <summary>CNPJ da SCP, preenchido somente pela própria SCP.</summary>
    [CampoSped(Ordem = 15, Tamanho = 14)]
    public Cnpj? CodScp { get; set; }
}
