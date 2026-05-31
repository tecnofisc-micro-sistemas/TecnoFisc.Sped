using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C870 — Itens do Resumo Diário dos Documentos (CF-E-SAT) (código 59).
/// Nível hierárquico 3, ocorrência 1:N por registro C860.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 158.
/// </summary>
[RegistroSped(Codigo = "C870", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC870 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C870";

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 5, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação. O código CFOP deve iniciar-se por "5".</summary>
    [CampoSped(Ordem = 6, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }
}
