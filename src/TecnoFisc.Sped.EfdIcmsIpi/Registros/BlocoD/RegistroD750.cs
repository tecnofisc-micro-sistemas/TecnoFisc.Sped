using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D750 — Escrituração consolidada da Nota Fiscal Fatura Eletrônica de Serviços de
/// Comunicação (NFCom, código 62). Pai da hierarquia consolidada D750 → D760 → D761.
/// Nível hierárquico 2, ocorrência 1:N por arquivo (somente saídas).
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 219 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 16 campos.
/// <b>V019:</b> adicionado o campo 17 <c>DED</c> (valor das deduções).
/// <para>
/// <b>Decisões de tipo (lazy <see cref="string"/>):</b> os campos <see cref="CodMod"/> (02) e
/// <see cref="Ser"/> (03) são declarados como <see cref="string"/> ao invés de tipos numéricos/value
/// objects dedicados para tolerar mudanças de tipo entre versões do leiaute — o tracker indica que
/// <c>COD_MOD</c> muda C→N em V018 e <c>SER</c> sofre flutuação análoga; manter texto preserva
/// fidelidade do conteúdo original para arquivos de qualquer versão. O PDF v3.2.2 lista
/// <c>COD_MOD</c> como C (canônico "62"). Modelo único aceita texto sempre — consumidor converte
/// se precisar.
/// </para>
/// <para>
/// <b>V018 (Guide 3.1.5 itens 8-10 + 3.1.6 item 6):</b> nova orientação geral do registro; tipo do
/// campo 03 <c>SER</c> muda de C para N (já modelado como <c>string?</c> lazy desde a criação em
/// V017 — nada a alterar no código, ver Decisões de tipo acima); chave fiscal do registro deixa de
/// incluir <c>COD_MUN_DEST</c> (regra de deduplicação fiscal fora do escopo do parser); nova
/// orientação no campo 07 <c>VL_DOC</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// <b>V019 (Guide 3.1.7 itens 6-7):</b> campos 15 <c>VL_PIS</c> e 16 <c>VL_COFINS</c> passam a
/// O (Obrigatório), antes OC; validação revisada no campo 07 <c>VL_DOC</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D750", Nivel = 2, Bloco = "D", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroD750 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D750";

    /// <summary>
    /// Código do modelo do documento fiscal conforme Tabela 4.1.1 — para NFCom o valor canônico é "62".
    /// Declarado como <see cref="string"/> (lazy) para tolerar mudanças de tipo entre versões do
    /// leiaute (V018 muda C→N; PDF v3.2.2 lista C). Modelo único aceita texto sempre.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>
    /// Série do documento fiscal.
    /// <para>
    /// <b>Decisão de tipo:</b> declarado como <see cref="string"/> (lazy) ao invés de <see cref="int"/>
    /// pelas mesmas razões do par <see cref="CodMod"/> — mudanças de tipo entre versões. Modelo
    /// único aceita texto sempre.
    /// </para>
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Data da emissão dos documentos consolidados no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Quantidade de documentos consolidados.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int QtdCons { get; set; }

    /// <summary>Indicador da forma de pagamento: 0-Pré-pago, 1-Pós-pago.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public FormaPagamentoConsolidadaNFCom IndPrepago { get; set; }

    /// <summary>Valor total dos documentos consolidados.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor acumulado da prestação de serviço.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor acumulado da prestação de serviço não tributada pelo ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServNt { get; set; }

    /// <summary>Valores cobrados em nome de terceiros.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTerc { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDesc { get; set; }

    /// <summary>Valor acumulado das despesas acessórias.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDa { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcms { get; set; }

    /// <summary>Valor acumulado do PIS (OC).</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor acumulado da COFINS (OC).</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>
    /// Valor das deduções. Campo adicionado em V019 — pacote read-only mantém o campo no modelo único;
    /// arquivos de versões anteriores simplesmente não preencherão a posição.
    /// </summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V019)]
    public decimal? Ded { get; set; }
}
