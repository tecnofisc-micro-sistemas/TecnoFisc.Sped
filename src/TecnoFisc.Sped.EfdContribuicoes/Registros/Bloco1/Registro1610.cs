using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1610 — Detalhamento da Contribuição Social Extemporânea – Cofins.
/// Nível hierárquico 3, ocorrência 1:N. Filho do Registro 1600. Detalha cada operação
/// que compõe a contribuição extemporânea, segregada por CNPJ, CST, participante e conta.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 404.
/// </summary>
[RegistroSped(Codigo = "1610", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1610 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1610";

    /// <summary>Número de inscrição do estabelecimento no CNPJ (Campo 04 do Registro 0140).</summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Código da Situação Tributária referente a COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    /// <summary>Código do participante (Campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Data da Operação (ddmmaaaa).</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOper { get; set; }

    /// <summary>Valor da Operação.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOper { get; set; }

    /// <summary>Base de cálculo da COFINS (em valor ou em quantidade).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VlBcCofins { get; set; }

    /// <summary>Alíquota da COFINS (em percentual ou em reais).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 10, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Descrição complementar do Documento/Operação.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0)]
    public string? DescCompl { get; set; }
}
