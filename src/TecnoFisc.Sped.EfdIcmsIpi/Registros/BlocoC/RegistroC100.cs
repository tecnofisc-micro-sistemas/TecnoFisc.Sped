using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C100 — NF (cód. 01), NF Avulsa (1B), NF-Produtor (04), NF-e (55) e NFC-e (65).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 59-64.
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.3 item 2):</b> nova orientação de preenchimento sobre escrituração de
/// operações com ICMS monofásico (Nota Orientativa 01/2023).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// <para>
/// <b>V017 (Guide 3.1.4 item 1):</b> nova validação de duplicidade do documento na combinação
/// (<c>IND_EMIT</c>, <c>COD_SIT</c>, <c>COD_PART</c>, <c>SER</c>, <c>NUM_DOC</c>), com exceção
/// quando <c>COD_MOD</c> for 55 ou 65.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// <b>V019 (Guide 3.1.9 itens 3-4):</b> regra de validação do campo 12 <c>VL_DOC</c> desabilitada;
/// nova observação relativa à Reforma Tributária do Consumo — documentos fiscais que escriturem
/// exclusivamente os novos tributos (IBS/CBS/IS) não devem ser informados em EFD ICMS-IPI.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C100", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C100";

    /// <summary>Indicador do tipo de operação: 0-Entrada, 1-Saída.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0-Emissão própria, 1-Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): emitente nas entradas, adquirente nas saídas.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (01, 1B, 04, 55, 65).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3)]
    public string? Ser { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Chave da NF-e (44 dígitos). Obrigatório para COD_MOD 55 e 65 de emissão própria.</summary>
    [CampoSped(Ordem = 9, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Data da entrada ou saída no formato DDMMAAAA. Obrigatório nas entradas; condicional nas saídas.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtES { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Indicador do tipo de pagamento: 0-À vista, 1-A prazo, 2-Outros (vigente a partir de 01/07/2012).</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorPagamento IndPgto { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Abatimento não tributado e não comercial (ex.: desconto ICMS nas remessas para ZFM).</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlAbatNt { get; set; }

    /// <summary>Valor total das mercadorias e serviços.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlMerc { get; set; }

    /// <summary>Indicador do tipo de frete (vigente a partir de 01/01/2018).</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFrete IndFrt { get; set; }

    /// <summary>Valor do frete indicado no documento fiscal.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlFrt { get; set; }

    /// <summary>Valor do seguro indicado no documento fiscal.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlSeg { get; set; }

    /// <summary>Valor de outras despesas acessórias.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlOutDa { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor da base de cálculo do ICMS substituição tributária.</summary>
    [CampoSped(Ordem = 23, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor do ICMS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Valor total do IPI.</summary>
    [CampoSped(Ordem = 25, Tamanho = 0, Decimais = 2)]
    public decimal? VlIpi { get; set; }

    /// <summary>Valor total do PIS.</summary>
    [CampoSped(Ordem = 26, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS.</summary>
    [CampoSped(Ordem = 27, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Valor total do PIS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 28, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisSt { get; set; }

    /// <summary>Valor total da COFINS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 29, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsSt { get; set; }
}
