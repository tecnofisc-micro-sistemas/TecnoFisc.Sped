using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F130 — Bens Incorporados ao Ativo Imobilizado – Operações Geradoras de Créditos com
/// Base no Valor de Aquisição/Contribuição. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 242.
/// </summary>
[RegistroSped(Codigo = "F130", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF130 : RegistroSped
{
    public override string Codigo => "F130";

    /// <summary>
    /// Código da Base de Cálculo do Crédito sobre Bens Incorporados ao Ativo Imobilizado,
    /// conforme Tabela 4.3.7. Valor fixo: 10 (crédito com base no valor de aquisição).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NatBcCred { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IdentificadorBemImobilizado IdentBemImob { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorOrigemCredito? IndOrigCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorUtilizacaoBemImobilizado IndUtilBemImob { get; set; }

    /// <summary>Mês/Ano de Aquisição dos Bens Incorporados ao Ativo Imobilizado (formato MMAAAA).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? MesOperAquis { get; set; }

    /// <summary>Valor de Aquisição dos Bens Incorporados ao Ativo Imobilizado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOperAquis { get; set; }

    /// <summary>Parcela do Valor de Aquisição a excluir da base de cálculo do Crédito.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? ParcOperNaoBcCred { get; set; }

    /// <summary>Valor da Base de Cálculo do Crédito sobre Bens Incorporados ao Ativo Imobilizado (Campo 07 – Campo 08).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCred { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 1, Obrigatorio = true)]
    public IndicadorNumeroParcelas IndNrParc { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 11, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    /// <summary>Base de Cálculo Mensal do Crédito de PIS/PASEP, conforme indicador informado no Campo 10.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 15, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    /// <summary>Base de Cálculo Mensal do Crédito da COFINS, conforme indicador informado no Campo 10.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 19, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Código do Centro de Custos.</summary>
    [CampoSped(Ordem = 20, Tamanho = 255)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição complementar do bem ou grupo de bens.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0)]
    public string? DescBemImob { get; set; }
}
