using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1391 - Producao Diaria da Usina. Nivel hierarquico 3, ocorrencia varios
/// por Registro 1390. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 280-281.
/// </summary>
[RegistroSped(Codigo = "1391", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1391 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1391";

    /// <summary>Data de producao.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRegistro { get; set; }

    /// <summary>Quantidade de insumo esmagado, em toneladas.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? QtdMoid { get; set; }

    /// <summary>Estoque inicial do dia.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal EstqIni { get; set; }

    /// <summary>Quantidade produzida no dia.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? QtdProduz { get; set; }

    /// <summary>Entrada decorrente da transformacao entre alcool anidro e hidratado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? EntAnidHid { get; set; }

    /// <summary>Outras entradas.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? OutrEntr { get; set; }

    /// <summary>Evaporacao ou quebra de peso.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? Perda { get; set; }

    /// <summary>Consumo proprio.</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.4 item 8):</b> novo valor válido "4" admitido no campo 09
    /// <c>CONS</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? Cons { get; set; }

    /// <summary>Saida para transformacao entre alcool anidro e hidratado.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? SaiAniHid { get; set; }

    /// <summary>Saidas totais.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? Saidas { get; set; }

    /// <summary>Estoque final do dia.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal EstqFin { get; set; }

    /// <summary>Estoque inicial de mel residual.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? EstqIniMel { get; set; }

    /// <summary>Producao e entradas de mel residual.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? ProdDiaMel { get; set; }

    /// <summary>Mel residual utilizado e saidas de mel.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? UtilMel { get; set; }

    /// <summary>Producao de alcool ou acucar proveniente do mel residual.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? ProdAlcMel { get; set; }

    /// <summary>Observacoes.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0)]
    public string? Obs { get; set; }

    /// <summary>Codigo do item do insumo utilizado.</summary>
    [CampoSped(Ordem = 18, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Tipo de residuo produzido.</summary>
    [CampoSped(Ordem = 19, Tamanho = 2, Obrigatorio = true)]
    public TipoResiduoProducaoUsina TpResiduo { get; set; }

    /// <summary>Quantidade de residuo produzido, em toneladas.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal QtdResiduo { get; set; }
}
