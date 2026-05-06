using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C800 — Cupom Fiscal Eletrônico (Código 59).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 177.
/// </summary>
[RegistroSped(Codigo = "C800", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC800 : RegistroSped
{
    public override string Codigo => "C800";

    /// <summary>Código do modelo do documento fiscal — Tabela 4.1.1. Valor válido: 59 (CF-e-SAT).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal — Tabela 4.1.2. Valores válidos: 00, 01, 02, 03.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 9, Obrigatorio = true)]
    public int NumCfe { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total do Cupom Fiscal Eletrônico — corresponde ao campo Valor Total do CF-e do leiaute do CF-e.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCfe { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>CNPJ (14 dígitos) ou CPF (11 dígitos) do destinatário.</summary>
    [CampoSped(Ordem = 9, Tamanho = 14)]
    public string? CnpjCpf { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 9)]
    public int? NrSat { get; set; }

    /// <summary>Chave do Cupom Fiscal Eletrônico (44 dígitos).</summary>
    [CampoSped(Ordem = 11, Tamanho = 44)]
    public string? ChvCfe { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlMerc { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlOutDa { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisSt { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsSt { get; set; }
}
