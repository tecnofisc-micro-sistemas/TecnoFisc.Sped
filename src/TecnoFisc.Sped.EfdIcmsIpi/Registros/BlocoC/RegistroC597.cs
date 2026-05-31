using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C597 — Outras Obrigações Tributárias, Ajustes e Informações de Valores
/// Provenientes do Documento Fiscal. Filho do C595; detalha obrigações tributárias,
/// ajustes e informações de valores do documento fiscal que podem alterar o cálculo do imposto.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 146.
/// </summary>
[RegistroSped(Codigo = "C597", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC597 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C597";

    /// <summary>Código do ajuste/benefício/incentivo, conforme tabela indicada no item 5.3.</summary>
    [CampoSped(Ordem = 2, Tamanho = 10, Obrigatorio = true)]
    public string? CodAj { get; set; }

    /// <summary>Descrição complementar do ajuste do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescrComplAj { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Base de cálculo do ICMS ou do ICMS ST.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do ICMS ou do ICMS ST.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Outros valores conforme Tabela 5.3.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlOutros { get; set; }
}
