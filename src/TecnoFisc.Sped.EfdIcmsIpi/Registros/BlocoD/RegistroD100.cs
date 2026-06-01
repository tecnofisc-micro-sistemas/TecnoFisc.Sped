using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D100 — NF Serviço de Transporte (07), Conhecimentos de Transporte Rodoviário (08, 8B),
/// Aquaviário (09), Aéreo (10), Ferroviário (11, 26, 27), CT-e (57, 67) e Bilhete de Passagem (63).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 171-175.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 itens 1-2, 29):</b> campos 24 <c>VL_PIS</c> e 25 <c>VL_COFINS</c>
/// passaram de OC (Obrigatório Condicional) para O (Obrigatório); validação do campo 11
/// <c>DT_DOC</c> foi alterada.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// <para>
/// <b>V019 (Guide 3.1.7 itens 9, 12-13 + 3.1.8 itens 1, 3):</b> nova orientação de preenchimento
/// dos campos 14 <c>CHV_CTE_REF</c>, 24 <c>COD_MUN_ORIG</c> e 25 <c>COD_MUN_DEST</c>; validações
/// revisadas nos campos 14 <c>CHV_CTE_REF</c> e 18 <c>VL_SERV</c>; inclusão da exceção nº 4 nas
/// regras gerais do registro.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// <b>V020 (Guide 3.2.1 item 5):</b> nova orientação de preenchimento do campo 25
/// <c>COD_MUN_DEST</c> (código do município de destino do serviço).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D100", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D100";

    /// <summary>Indicador do tipo de operação: 0-Aquisição, 1-Prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0-Emissão própria, 1-Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): prestador na aquisição, tomador na prestação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (07, 08, 8B, 09, 10, 11, 26, 27, 57, 63, 67).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3)]
    public string? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 9, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Chave do CT-e ou Bilhete de Passagem Eletrônico (44 dígitos).</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChvCte { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Data da aquisição ou da prestação do serviço no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 12, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtAP { get; set; }

    /// <summary>Tipo de CT-e ou BP-e conforme Manual de Integração.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1)]
    public int? TpCTe { get; set; }

    /// <summary>Chave do documento eletrônico substituído (44 dígitos).</summary>
    [CampoSped(Ordem = 14, Tamanho = 44)]
    public ChaveAcesso? ChvCteRef { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Indicador do tipo do frete: 0-Emitente, 1-Destinatário/Remetente, 2-Terceiros, 9-Sem frete.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1)]
    public IndicadorFrete? IndFrt { get; set; }

    /// <summary>Valor total da prestação de serviço.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor não-tributado.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlNt { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 22, Tamanho = 6)]
    public string? CodInf { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 23, Tamanho = 0)]
    public string? CodCta { get; set; }

    /// <summary>Código do município de origem do serviço conforme tabela IBGE (7 dígitos; 9999999 = Exterior).</summary>
    [CampoSped(Ordem = 24, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino do serviço conforme tabela IBGE (7 dígitos; 9999999 = Exterior).</summary>
    [CampoSped(Ordem = 25, Tamanho = 7)]
    public int? CodMunDest { get; set; }
}
