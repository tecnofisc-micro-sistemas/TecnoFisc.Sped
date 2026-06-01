using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C321 — Itens do Resumo Diário dos Documentos (código 02).
/// Detalha por item de mercadoria a consolidação diária dos valores das notas fiscais de
/// venda ao consumidor, não emitidas por ECF.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 109.
/// </summary>
[RegistroSped(Codigo = "C321", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC321 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C321";

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade acumulada do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3)]
    public decimal? Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 4, Tamanho = 6)]
    public string? Unid { get; set; }

    /// <summary>Valor acumulado do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlItem { get; set; }

    /// <summary>Valor do desconto acumulado. Valor meramente informativo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS debitado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor acumulado do PIS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor acumulado da COFINS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
