using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C495 — Resumo Mensal de Itens do ECF por Estabelecimento (cód. 02 e 2D).
/// Resumo único por item de mercadoria, obrigatório para contribuintes domiciliados na Bahia até 31/12/2013.
/// Nível hierárquico 2, ocorrência vários por C001.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 132.
/// </summary>
[RegistroSped(Codigo = "C495", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC495 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C495";

    /// <summary>Alíquota do ICMS — carga tributária efetiva da operação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade acumulada do item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? Qtd { get; set; }

    /// <summary>Quantidade cancelada acumulada, no caso de cancelamento parcial de item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? QtdCanc { get; set; }

    /// <summary>Unidade do item (Campo 02 do registro 0190).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor acumulado do item.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlItem { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor acumulado dos cancelamentos.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlCanc { get; set; }

    /// <summary>Valor acumulado dos acréscimos.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlAcmo { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor das saídas isentas do ICMS.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlIsen { get; set; }

    /// <summary>Valor das saídas sob não-incidência ou não-tributadas pelo ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlNt { get; set; }

    /// <summary>Valor das saídas de mercadorias adquiridas com substituição tributária do ICMS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }
}
