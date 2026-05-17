using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C178 — Complemento de Item: Operações com Produtos Sujeitos à Tributação de IPI
/// por Unidade ou Quantidade de Produto (cód. 01, 55).
/// Nível hierárquico 4, ocorrência 1:1 (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 90.
/// </summary>
[RegistroSped(Codigo = "C178", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC178 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C178";

    /// <summary>Código da classe de enquadramento do IPI, conforme Tabela 4.5.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 5, Obrigatorio = true)]
    public string? ClEnq { get; set; }

    /// <summary>Valor por unidade padrão de tributação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlUnid { get; set; }

    /// <summary>Quantidade total de produtos na unidade padrão de tributação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal? QuantPad { get; set; }
}
