using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1501 — Apuração de Crédito Extemporâneo - Documentos e Operações de Períodos
/// Anteriores – Cofins. Nível hierárquico 3, ocorrência 1:N. Filho do Registro 1500.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 399.
/// </summary>
[RegistroSped(Codigo = "1501", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1501 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1501";

    /// <summary>Código do participante (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 3)]
    public string? SubSer { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 9)]
    public int? NumDoc { get; set; }

    /// <summary>Data da operação (ddmmaaaa).</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOper { get; set; }

    /// <summary>Chave da Nota Fiscal Eletrônica.</summary>
    [CampoSped(Ordem = 9, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    /// <summary>Valor total da operação.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOper { get; set; }

    /// <summary>Código fiscal de operação e prestação.</summary>
    [CampoSped(Ordem = 11, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Código da Base de Cálculo do Crédito, conforme Tabela 4.3.7.</summary>
    [CampoSped(Ordem = 12, Tamanho = 2, Obrigatorio = true)]
    public string? NatBcCred { get; set; }

    /// <summary>Indicador da origem do crédito: 0 = Mercado Interno; 1 = Importação.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemCredito IndOrigCred { get; set; }

    /// <summary>Código da Situação Tributária referente ao COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 14, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    /// <summary>Base de Cálculo do Crédito de COFINS (em valor ou em quantidade).</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    /// <summary>Alíquota do COFINS (em percentual ou em reais).</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    /// <summary>Valor do Crédito de COFINS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 18, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Código do Centro de Custos.</summary>
    [CampoSped(Ordem = 19, Tamanho = 255)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição complementar do documento/operação.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0)]
    public string? DescCompl { get; set; }

    /// <summary>Mês/Ano da escrituração em que foi registrado o documento/operação (formato MMAAAA; preenchido somente para crédito pelo método da Apropriação Direta).</summary>
    [CampoSped(Ordem = 21, Tamanho = 6)]
    public string? PerEscrit { get; set; }

    /// <summary>CNPJ do estabelecimento gerador do crédito extemporâneo (campo 04 do Registro 0140).</summary>
    [CampoSped(Ordem = 22, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }
}
