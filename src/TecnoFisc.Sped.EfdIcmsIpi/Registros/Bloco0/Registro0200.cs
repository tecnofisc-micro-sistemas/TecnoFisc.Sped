using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0200 — Tabela de Identificação do Item (Produtos e Serviços). Nível hierárquico 2,
/// ocorrência vários por arquivo. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 33-35.
/// </summary>
[RegistroSped(Codigo = "0200", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0200";

    /// <summary>Código do item (livre atribuição do informante, único por período). Deve existir em pelo menos um registro dos demais blocos ou no Registro 0220.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Descrição do item. Vedadas descrições genéricas para o mesmo item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrItem { get; set; }

    /// <summary>Representação alfanumérica do código de barra do produto (GTIN-8, -12, -13 ou -14), se houver.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? CodBarra { get; set; }

    /// <summary>Código anterior do item. Não preencher — quando houver, informar no Registro 0205.</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodAntItem { get; set; }

    /// <summary>Unidade de medida utilizada na quantificação de estoques. Deve existir no Registro 0190, campo UNID.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public string? UnidInv { get; set; }

    /// <summary>Tipo do item — atividades industriais, comerciais e de serviços.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public TipoItem TipoItem { get; set; }

    /// <summary>Código da Nomenclatura Comum do Mercosul (8 dígitos). Obrigatório para empresas industriais, substitutas tributárias e que realizem exportação/importação.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8)]
    public Ncm? CodNcm { get; set; }

    /// <summary>Código EX conforme a TIPI (Tabela de Incidência do IPI).</summary>
    [CampoSped(Ordem = 9, Tamanho = 3)]
    public string? ExIpi { get; set; }

    /// <summary>Código do gênero do item conforme Tabela 4.2.1 do Ato COTEPE/ICMS nº 44/2018 (Capítulos da NCM; "00" = Serviço). Obrigatório na aquisição de produtos primários.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2)]
    public GeneroItem? CodGen { get; set; }

    /// <summary>Código do serviço conforme lista do Anexo I da Lei Complementar Federal nº 116/03, formato NN.NN.</summary>
    [CampoSped(Ordem = 11, Tamanho = 5)]
    public string? CodLst { get; set; }

    /// <summary>Alíquota de ICMS aplicável ao item nas operações internas (incluindo FCP quando aplicável).</summary>
    [CampoSped(Ordem = 12, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Código Especificador da Substituição Tributária — CEST (Convênio ICMS 52/2017, válido a partir de 01/01/2017).</summary>
    [CampoSped(Ordem = 13, Tamanho = 7)]
    public string? Cest { get; set; }
}
