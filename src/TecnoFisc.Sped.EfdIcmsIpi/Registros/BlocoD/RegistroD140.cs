using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D140 — Complemento do CT Aquaviário de Cargas (Código 09).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 179.
/// </summary>
[RegistroSped(Codigo = "D140", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD140 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D140";

    /// <summary>Código do participante consignatário (campo 02 do Registro 0150), se houver.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPartConsg { get; set; }

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 4, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Indicador do tipo do veículo transportador.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorTipoVeiculoAquaviario? IndVeic { get; set; }

    /// <summary>Identificação da embarcação (IRIM ou Registro CPP).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? VeicId { get; set; }

    /// <summary>Indicador do tipo da navegação.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorTipoNavegacao? IndNav { get; set; }

    /// <summary>Número da viagem.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public int? Viagem { get; set; }

    /// <summary>Valor líquido do frete.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrtLiq { get; set; }

    /// <summary>Valor das despesas portuárias.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlDespPort { get; set; }

    /// <summary>Valor das despesas com carga e descarga.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlDespCarDesc { get; set; }

    /// <summary>Outros valores.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlOut { get; set; }

    /// <summary>Valor bruto do frete.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrtBrt { get; set; }

    /// <summary>Valor adicional do frete para renovação da Marinha Mercante.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrtMm { get; set; }
}
