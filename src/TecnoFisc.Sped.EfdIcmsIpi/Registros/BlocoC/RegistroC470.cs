using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C470 — Itens do Documento Fiscal Emitido por ECF (código 02 e 2D).
/// Informa os itens dos documentos fiscais emitidos por equipamentos ECF, totalizados na Redução Z.
/// Nível hierárquico 5, ocorrência 1:N por C460.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 129.
/// </summary>
[RegistroSped(Codigo = "C470", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC470 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C470";

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Quantidade cancelada, no caso de cancelamento parcial de item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3)]
    public decimal? QtdCanc { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor total do item.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação. O código CFOP deve iniciar-se por "5".</summary>
    [CampoSped(Ordem = 8, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS — carga tributária efetiva em percentual.</summary>
    [CampoSped(Ordem = 9, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do PIS. Dispensado para contribuintes que entregam EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS. Dispensado para contribuintes que entregam EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
