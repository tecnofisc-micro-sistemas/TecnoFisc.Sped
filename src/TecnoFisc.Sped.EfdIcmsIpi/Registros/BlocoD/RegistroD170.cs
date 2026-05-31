using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D170 — Complemento do Conhecimento Multimodal de Cargas (Código 26).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 175-176.
/// </summary>
[RegistroSped(Codigo = "D170", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD170 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D170";

    /// <summary>Código do participante consignatário (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPartConsg { get; set; }

    /// <summary>Código do participante redespachante (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodPartRed { get; set; }

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE. Preencher com 9999999 se Exterior.</summary>
    [CampoSped(Ordem = 4, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino, conforme tabela IBGE. Preencher com 9999999 se Exterior.</summary>
    [CampoSped(Ordem = 5, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Número de registro do operador de transporte multimodal junto à ANTT (8 dígitos).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? Otm { get; set; }

    /// <summary>Indicador da natureza do frete: se o CTMC pode ser negociado em instituição financeira.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public IndicadorNaturezaFreteMultimodal? IndNatFrt { get; set; }

    /// <summary>Valor líquido do frete.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlLiqFrt { get; set; }

    /// <summary>Valor do GRIS (gerenciamento de risco).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlGris { get; set; }

    /// <summary>Somatório dos valores de pedágio.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlPdg { get; set; }

    /// <summary>Outros valores.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlOut { get; set; }

    /// <summary>Valor total do frete.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrt { get; set; }

    /// <summary>Placa de identificação do veículo.</summary>
    [CampoSped(Ordem = 13, Tamanho = 7)]
    public string? VeicId { get; set; }

    /// <summary>Sigla da UF da placa do veículo.</summary>
    [CampoSped(Ordem = 14, Tamanho = 2)]
    public string? UfId { get; set; }
}
