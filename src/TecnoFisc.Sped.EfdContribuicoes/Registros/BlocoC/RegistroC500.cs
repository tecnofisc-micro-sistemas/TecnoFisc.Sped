using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C500 — NF/Conta Energia Elétrica (06), NF3e (66), Água (29), Gás (28) e NF-e (55)
/// – Documentos de Entrada / Aquisição com Crédito.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 165.
/// </summary>
[RegistroSped(Codigo = "C500", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC500 : RegistroSped
{
    public override string Codigo => "C500";

    /// <summary>Código do participante — campo 02 do Registro 0150.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal — Tabela 4.1.1. Valores válidos: 06, 28, 29, 55, 66.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 3)]
    public int? Sub { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 9, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtEnt { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da informação complementar do documento fiscal — campo 02 do Registro 0450.</summary>
    [CampoSped(Ordem = 12, Tamanho = 6)]
    public string? CodInf { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Chave do Documento Fiscal Eletrônico (44 dígitos). Obrigatório a partir de 01/01/2020 quando COD_MOD = 66 ou 55.</summary>
    [CampoSped(Ordem = 15, Tamanho = 44)]
    public ChaveAcesso? ChvDoce { get; set; }
}
