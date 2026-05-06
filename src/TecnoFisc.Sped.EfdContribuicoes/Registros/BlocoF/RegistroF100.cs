using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F100 — Demais Documentos e Operações Geradoras de Contribuição e Créditos.
/// Nível hierárquico 3, ocorrência 1:N. Escritura receitas, custos, despesas e encargos não
/// informados nos Blocos A, C e D. Conforme Guia Prático EFD Contribuições v1.35, p. 230.
/// </summary>
[RegistroSped(Codigo = "F100", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF100 : RegistroSped
{
    public override string Codigo => "F100";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoF100 IndOper { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOper { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOper { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente à Cofins, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 11, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da Base de Cálculo dos Créditos, conforme Tabela 4.3.7.</summary>
    [CampoSped(Ordem = 15, Tamanho = 2)]
    public string? NatBcCred { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 1)]
    public IndicadorOrigemCredito? IndOrigCred { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 17, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Código do Centro de Custos.</summary>
    [CampoSped(Ordem = 18, Tamanho = 255)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição do Documento/Operação.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0)]
    public string? DescDocOper { get; set; }
}
