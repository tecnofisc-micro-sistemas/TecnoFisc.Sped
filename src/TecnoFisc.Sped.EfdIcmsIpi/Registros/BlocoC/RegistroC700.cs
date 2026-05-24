using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C700 — Consolidação dos Documentos NF/Conta Energia Elétrica (cód. 06) Emitidas em
/// Via Única (empresas obrigadas ao Convênio ICMS 115/03) e NF/Conta de Fornecimento de Gás
/// Canalizado (cód. 28). A apresentação deste registro exclui o Registro C600.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 148-149.
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.2 itens 1-3):</b> passa a admitir também a NF3-e (modelo 66) no
/// campo 02 <c>COD_MOD</c>; obrigatoriedade dos campos 08 <c>NOM_MEST</c> e 09
/// <c>CHV_COD_DIG</c> alterada de "O" para "OC"; nova orientação de preenchimento para
/// os campos 06 <c>DT_DOC_INI</c>, 07 <c>DT_DOC_FIN</c>, 08 <c>NOM_MEST</c> e 09
/// <c>CHV_COD_DIG</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// <para>
/// <b>V017 (Guide 3.1.4 item 2):</b> nova orientação determinando que NF3-e (modelo 66)
/// sem CST e sem energia injetada não deve ser escriturada neste registro.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// <b>V018 (Guide 3.1.5 item 23):</b> nova orientação geral do registro reforçando o uso
/// para UF cuja legislação permite a escrituração consolidada da NF3-e (modelo 66) e da
/// NF/Conta de Energia Elétrica (modelo 06) sob o Convênio ICMS 115/2003. Complementa as
/// alterações V017 acima — não as substitui.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// <b>V019 (Guide 3.1.7 item 1):</b> validação geral do registro revisada (consolidação da
/// regra de uso para NF3-e modelo 66 sob Convênio ICMS 115/2003 introduzida em V017/V018).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C700", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC700 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C700";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 06, 28.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Número de ordem inicial dos documentos consolidados. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 4, Tamanho = 9, Obrigatorio = true)]
    public long? NroOrdIni { get; set; }

    /// <summary>Número de ordem final dos documentos consolidados. Deve ser maior ou igual ao NRO_ORD_INI.</summary>
    [CampoSped(Ordem = 5, Tamanho = 9, Obrigatorio = true)]
    public long? NroOrdFin { get; set; }

    /// <summary>Data de emissão inicial dos documentos ou data inicial de vencimento da fatura, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDocIni { get; set; }

    /// <summary>Data de emissão final dos documentos ou data final de vencimento da fatura, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDocFin { get; set; }

    /// <summary>Nome do arquivo Mestre de Documento Fiscal, conforme item 4.5 do Anexo Único do Convênio ICMS 115/2003.</summary>
    [CampoSped(Ordem = 8, Tamanho = 33, Obrigatorio = true)]
    public string? NomMest { get; set; }

    /// <summary>Chave de codificação digital do arquivo Mestre de Documento Fiscal, conforme Parágrafo Único da Cláusula Segunda do Convênio ICMS 115/2003.</summary>
    [CampoSped(Ordem = 9, Tamanho = 32, Obrigatorio = true)]
    public string? ChvCodDig { get; set; }
}
