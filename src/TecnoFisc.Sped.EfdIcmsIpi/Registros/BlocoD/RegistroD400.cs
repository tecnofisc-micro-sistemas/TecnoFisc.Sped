using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D400 — Resumo do Movimento Diário - RMD (código 18).
/// Consolida diariamente bilhetes de passagem rodoviário (13), aquaviário (14),
/// passagem e bagagem (15) e ferroviário (16) não emitidos por ECF.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 188.
/// </summary>
[RegistroSped(Codigo = "D400", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD400 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D400";

    /// <summary>Código do participante — agência, filial ou posto (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (valor 18 — RMD).</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Número do documento fiscal resumo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor acumulado da prestação de serviço.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor total da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor total do ICMS.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0)]
    public string? CodCta { get; set; }
}
