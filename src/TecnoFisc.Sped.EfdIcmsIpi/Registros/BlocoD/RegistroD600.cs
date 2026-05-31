using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D600 — Consolidação da Prestação de Serviços — NF Serviço de Comunicação (cód. 21) e
/// Serviço de Telecomunicação (cód. 22) para empresas NÃO obrigadas ao Convênio ICMS 115/2003.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 197-198.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 3):</b> campos <c>VL_BC_ICMS</c> e <c>VL_ICMS</c> passaram de
/// O (Obrigatório) para OC (Obrigatório Condicional).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "D600", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D600";

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1 (21 ou 22).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código do município dos terminais faturados, conforme a tabela IBGE.</summary>
    [CampoSped(Ordem = 3, Tamanho = 7, Obrigatorio = true)]
    public int? CodMun { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Código de classe de consumo dos serviços de comunicação ou telecomunicação, conforme a Tabela 4.4.4.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public int? CodCons { get; set; }

    /// <summary>Quantidade de documentos consolidados neste registro.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int? QtdCons { get; set; }

    /// <summary>Data dos documentos consolidados no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total acumulado dos documentos fiscais.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor acumulado das prestações de serviços tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor acumulado dos serviços não-tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valores cobrados em nome de terceiros.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor acumulado das despesas acessórias.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcms { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
