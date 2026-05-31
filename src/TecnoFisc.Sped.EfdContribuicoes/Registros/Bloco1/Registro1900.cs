using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1900 — Consolidação dos Documentos Emitidos no Período por Pessoa Jurídica Submetida
/// ao Regime de Tributação com Base no Lucro Presumido – Regime de Caixa ou de Competência.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Obrigatório a partir de abril de 2013.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 410.
/// </summary>
[RegistroSped(Codigo = "1900", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1900";

    /// <summary>CNPJ do estabelecimento emitente dos documentos geradores de receita.</summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Código do modelo do documento fiscal (Tabela 4.1.1; 98 = NF de Serviço ISSQN; 99 = Outros Documentos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 20)]
    public int? SubSer { get; set; }

    /// <summary>Código da situação do documento fiscal: 00 = regular; 02 = cancelado; 99 = outros.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2)]
    public string? CodSit { get; set; }

    /// <summary>Valor total da receita, conforme documentos emitidos no período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotRec { get; set; }

    /// <summary>Quantidade total de documentos emitidos no período.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public long? QuantDoc { get; set; }

    /// <summary>CST do PIS/Pasep, conforme Tabela II IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? CstPis { get; set; }

    /// <summary>CST da Cofins, conforme Tabela III IN RFB nº 1.009/2010.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2)]
    public string? CstCofins { get; set; }

    /// <summary>Código fiscal de operação e prestação.</summary>
    [CampoSped(Ordem = 11, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Informações complementares.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? InfCompl { get; set; }

    /// <summary>Código da conta analítica contábil representativa da receita.</summary>
    [CampoSped(Ordem = 13, Tamanho = 255)]
    public string? CodCta { get; set; }
}
