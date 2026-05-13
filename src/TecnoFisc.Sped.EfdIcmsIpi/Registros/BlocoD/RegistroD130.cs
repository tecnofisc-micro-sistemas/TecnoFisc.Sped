using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D130 — Complemento do CT Rodoviário de Cargas (Código 08) e CT de Cargas Avulso (Código 8B).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 170.
/// </summary>
[RegistroSped(Codigo = "D130", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD130 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D130";

    /// <summary>Código do participante consignatário (campo 02 do Registro 0150), se houver.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPartConsg { get; set; }

    /// <summary>Código do participante redespachado (campo 02 do Registro 0150), se houver.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodPartRed { get; set; }

    /// <summary>Indicador do tipo do frete da operação de redespacho.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorFreteRedespacho? IndFrtRed { get; set; }

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 5, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 6, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Placa de identificação do veículo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 7)]
    public string? VeicId { get; set; }

    /// <summary>Valor líquido do frete.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlLiqFrt { get; set; }

    /// <summary>Soma de valores de Sec/Cat (serviços de coleta/custo adicional de transporte).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlSecCat { get; set; }

    /// <summary>Soma de valores de despacho.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesp { get; set; }

    /// <summary>Soma dos valores de pedágio.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlPedg { get; set; }

    /// <summary>Outros valores.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlOut { get; set; }

    /// <summary>Valor total do frete.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrt { get; set; }

    /// <summary>Sigla da UF da placa do veículo.</summary>
    [CampoSped(Ordem = 14, Tamanho = 2)]
    public string? UfId { get; set; }
}
