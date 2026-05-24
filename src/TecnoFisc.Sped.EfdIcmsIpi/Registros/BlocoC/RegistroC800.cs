using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C800 — Cupom Fiscal Eletrônico – SAT (CF-e-SAT) (código 59).
/// Deve ser gerado para cada CF-e-SAT emitido por equipamento SAT-CF-e,
/// conforme Ajuste SINIEF nº 11, de 24 de setembro de 2010.
/// Nível hierárquico 2, ocorrência vários.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 151.
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0 item 3):</b> nova Exceção nº 2 incluída na instrução do registro
/// (escrituração de CF-e-SAT em situações específicas).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C800", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC800 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C800";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valor válido: 59 (CF-e-SAT).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal, conforme Tabela 4.1.2. Valores válidos: 00, 01, 02, 03.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Número do Cupom Fiscal Eletrônico.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public int NumCfe { get; set; }

    /// <summary>Data da emissão do Cupom Fiscal Eletrônico, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total do Cupom Fiscal Eletrônico.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCfe { get; set; }

    /// <summary>Valor total do PIS. Dispensado para contribuintes que entregam EFD-Contribuições do mesmo período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS. Dispensado para contribuintes que entregam EFD-Contribuições do mesmo período.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>CNPJ (14 dígitos) ou CPF (11 dígitos) do destinatário.</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.4 item 6):</b> nova validação no campo 09 <c>CNPJ_CPF</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 9, Tamanho = 14)]
    public string? CnpjCpf { get; set; }

    /// <summary>Número de Série do equipamento SAT.</summary>
    [CampoSped(Ordem = 10, Tamanho = 9, Obrigatorio = true)]
    public int NrSat { get; set; }

    /// <summary>Chave do Cupom Fiscal Eletrônico (44 caracteres).</summary>
    [CampoSped(Ordem = 11, Tamanho = 44, Obrigatorio = true)]
    public string? ChvCfe { get; set; }

    /// <summary>Valor total de descontos.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDesc { get; set; }

    /// <summary>Valor total das mercadorias e serviços.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlMerc { get; set; }

    /// <summary>Valor total de outras despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDa { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcms { get; set; }

    /// <summary>Valor total do PIS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlPisSt { get; set; }

    /// <summary>Valor total da COFINS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofinsSt { get; set; }
}
