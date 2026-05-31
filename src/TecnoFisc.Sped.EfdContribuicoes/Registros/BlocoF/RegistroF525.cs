using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F525 — Composição da Receita Escriturada no Período – Detalhamento da Receita
/// Recebida pelo Regime de Caixa. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 266.
/// </summary>
[RegistroSped(Codigo = "F525", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF525 : RegistroSped
{
    public override string Codigo => "F525";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRec { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorComposicaoReceitaCaixa IndRec { get; set; }

    /// <summary>CNPJ/CPF do participante (quando IndRec = Clientes ou AdminCartao).</summary>
    [CampoSped(Ordem = 4, Tamanho = 14)]
    public string? CnpjCpf { get; set; }

    /// <summary>Número do título de crédito ou documento fiscal (quando IndRec = TituloCredito ou DocumentoFiscal).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? NumDoc { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200) quando IndRec = ItemVendido.</summary>
    [CampoSped(Ordem = 6, Tamanho = 60)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecDet { get; set; }

    /// <summary>Código da Situação Tributária do PIS/Pasep.</summary>
    [CampoSped(Ordem = 8, Tamanho = 2)]
    public string? CstPis { get; set; }

    /// <summary>Código da Situação Tributária da Cofins.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? CstCofins { get; set; }

    /// <summary>Informação complementar.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? InfoCompl { get; set; }

    /// <summary>Código da conta analítica contábil representativa da receita recebida.</summary>
    [CampoSped(Ordem = 11, Tamanho = 255)]
    public string? CodCta { get; set; }
}
