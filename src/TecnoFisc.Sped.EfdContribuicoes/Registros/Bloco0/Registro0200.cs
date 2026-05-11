using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0200 — Tabela de Identificação do Item (Produtos e Serviços). Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 83-85.
/// </summary>
[RegistroSped(Codigo = "0200", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0200 : RegistroSped
{
    public override string Codigo => "0200";

    /// <summary>Código do item (livre atribuição do informante).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Descrição do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrItem { get; set; }

    /// <summary>Representação alfanumérica do código de barra do produto, se houver.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? CodBarra { get; set; }

    /// <summary>Código anterior do item com relação à última informação apresentada.</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodAntItem { get; set; }

    /// <summary>Unidade de medida utilizada na quantificação de estoques. Deve existir no Registro 0190.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? UnidInv { get; set; }

    /// <summary>Tipo do item — atividades industriais, comerciais e de serviços.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public TipoItem TipoItem { get; set; }

    /// <summary>Código NCM conforme Nomenclatura Comum do Mercosul (8 dígitos).</summary>
    [CampoSped(Ordem = 8, Tamanho = 8)]
    public Ncm? CodNcm { get; set; }

    /// <summary>Código EX conforme a TIPI.</summary>
    [CampoSped(Ordem = 9, Tamanho = 3)]
    public string? ExIpi { get; set; }

    /// <summary>Código do gênero do item conforme Tabela 4.2.1 do Manual de Orientação do Leiaute.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2)]
    public string? CodGen { get; set; }

    /// <summary>Código do serviço conforme lista do Anexo I da Lei Complementar Federal nº 116/03 (4 ou 5 caracteres).</summary>
    [CampoSped(Ordem = 11, Tamanho = 0)]
    public string? CodLst { get; set; }

    /// <summary>Alíquota de ICMS aplicável ao item nas operações internas.</summary>
    [CampoSped(Ordem = 12, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }
}
