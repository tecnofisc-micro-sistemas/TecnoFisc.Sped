using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N615 - base de cálculo dos incentivos fiscais.</summary>
[RegistroSped(Codigo = "N615", Nivel = 3, Bloco = "N")]
public sealed partial class RegistroN615 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N615";

    /// <summary>Base de cálculo declarada dos incentivos fiscais.</summary>
    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal BaseCalc { get; set; }

    /// <summary>Percentual declarado do incentivo FINOR.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PerIncenFinor { get; set; }

    /// <summary>Valor líquido declarado do incentivo FINOR.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlLiqIncenFinor { get; set; }

    /// <summary>Percentual declarado do incentivo FINAM.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PerIncenFinam { get; set; }

    /// <summary>Valor líquido declarado do incentivo FINAM.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlLiqIncenFinam { get; set; }

    /// <summary>Valor total declarado dos incentivos.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotal { get; set; }
}
