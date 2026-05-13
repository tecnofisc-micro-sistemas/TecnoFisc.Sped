using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E510 — Consolidação dos Valores de IPI.
/// Nível hierárquico 3, ocorrência 1:N por período. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 232.
/// </summary>
[RegistroSped(Codigo = "E510", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E510";

    /// <summary>Código Fiscal de Operação e Prestação do agrupamento de itens.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Código da Situação Tributária referente ao IPI, conforme Tabela 4.3.2.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CstIpi { get; set; }

    /// <summary>Valor contábil referente ao CFOP e ao Código de Tributação do IPI.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContIpi { get; set; }

    /// <summary>Valor da base de cálculo do IPI para operações tributadas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIpi { get; set; }

    /// <summary>Valor do IPI referente ao CFOP e ao Código de Tributação do IPI.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIpi { get; set; }
}
