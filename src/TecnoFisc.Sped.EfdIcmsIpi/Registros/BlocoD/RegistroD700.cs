using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D700 — Nota Fiscal Fatura Eletrônica de Serviços de Comunicação (NFCom, código 62).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 213-215 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 31 campos.
/// <b>V019:</b> adicionado o campo 32 <c>DED</c> (valor das deduções).
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D700", Nivel = 2, Bloco = "D", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroD700 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D700";

    /// <summary>Indicador do tipo de operação: 0-Entrada, 1-Saída.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0-Emissão própria, 1-Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150) — obrigatório nas entradas.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>
    /// Código do modelo do documento fiscal conforme Tabela 4.1.1 — para NFCom o valor canônico é "62".
    /// Declarado como <see cref="string"/> (não como value object <c>ModeloDocumento</c>) seguindo o padrão
    /// dos demais registros do bloco D, simplificando até existir um value object dedicado.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>
    /// Série do documento fiscal.
    /// <para>
    /// <b>Decisão de tipo:</b> declarado como <see cref="string"/> (lazy) ao invés de <see cref="int"/>
    /// para tolerar mudanças de tipo entre versões do leiaute (V020 muda N→C; o PDF v3.2.2 já lista C).
    /// Modelo único aceita texto sempre — consumidor converte se precisar.
    /// </para>
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Data de entrada ou de saída (campo OC, somente para entradas) no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtES { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor da prestação de serviço.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlServ { get; set; }

    /// <summary>Valor da prestação de serviço não tributada pelo ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valores cobrados em nome de terceiros.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor das despesas acessórias.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 19, Tamanho = 6)]
    public string? CodInf { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>
    /// Chave da NFCom (44 dígitos). Declarado como <see cref="string"/> — NFCom possui chave própria
    /// mod-44, e o value object <c>ChaveAcesso</c> foi modelado para NF-e/NFC-e/CT-e específicamente.
    /// Mantemos <see cref="string"/> até existir value object dedicado para NFCom.
    /// </summary>
    [CampoSped(Ordem = 22, Tamanho = 44, Obrigatorio = true)]
    public string? ChvDOCe { get; set; }

    /// <summary>Finalidade da emissão do documento eletrônico (0-Normal, 3-Substituição, 4-Ajuste).</summary>
    [CampoSped(Ordem = 23, Tamanho = 1, Obrigatorio = true)]
    public FinalidadeDocumentoEletronicoNFCom FinDOCe { get; set; }

    /// <summary>Tipo de faturamento (0-Normal, 1-Centralizado, 2-Cofaturamento).</summary>
    [CampoSped(Ordem = 24, Tamanho = 1, Obrigatorio = true)]
    public TipoFaturamentoNFCom TipFat { get; set; }

    /// <summary>Código do modelo do documento fiscal referenciado.</summary>
    [CampoSped(Ordem = 25, Tamanho = 2)]
    public string? CodModDocRef { get; set; }

    /// <summary>Chave do documento eletrônico referenciado (44 dígitos).</summary>
    [CampoSped(Ordem = 26, Tamanho = 44)]
    public string? ChvDOCeRef { get; set; }

    /// <summary>Hash do documento referenciado quando não possuir chave eletrônica.</summary>
    [CampoSped(Ordem = 27, Tamanho = 32)]
    public string? HashDocRef { get; set; }

    /// <summary>Série do documento referenciado.</summary>
    [CampoSped(Ordem = 28, Tamanho = 4)]
    public string? SerDocRef { get; set; }

    /// <summary>Número do documento referenciado.</summary>
    [CampoSped(Ordem = 29, Tamanho = 9)]
    public int? NumDocRef { get; set; }

    /// <summary>Mês e ano de emissão do documento referenciado no formato MMAAAA.</summary>
    [CampoSped(Ordem = 30, Tamanho = 6, Formato = "MMyyyy")]
    public DateOnly? MesDocRef { get; set; }

    /// <summary>Código do município de destino do serviço conforme tabela IBGE (7 dígitos; obrigatório nas saídas).</summary>
    [CampoSped(Ordem = 31, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>
    /// Valor das deduções. Campo adicionado em V019 — pacote read-only mantém o campo no modelo único;
    /// arquivos de versões anteriores simplesmente não preencherão a posição.
    /// </summary>
    [CampoSped(Ordem = 32, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V019)]
    public decimal? Ded { get; set; }
}
