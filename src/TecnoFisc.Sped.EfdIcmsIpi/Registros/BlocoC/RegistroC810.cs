using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C810 — Itens do Documento do Cupom Fiscal Eletrônico – SAT (CF-E-SAT) (código 59).
/// Deve ser informado apenas quando houver um registro filho C815.
/// Nível hierárquico 3, ocorrência 1:N por registro C800.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 153.
/// </summary>
[RegistroSped(Codigo = "C810", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC810 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C810";

    /// <summary>Número do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 5, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor total do item (mercadorias ou serviços).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação. O código CFOP deve iniciar-se por "5".</summary>
    [CampoSped(Ordem = 8, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }
}
