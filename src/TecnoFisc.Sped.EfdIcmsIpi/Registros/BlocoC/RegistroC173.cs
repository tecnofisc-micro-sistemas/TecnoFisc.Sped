using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C173 — Complemento de Item: Operações com Medicamentos (cód. 01, 55).
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 83.
/// </summary>
[RegistroSped(Codigo = "C173", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC173 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C173";

    /// <summary>Número do lote de fabricação do medicamento.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? LoteMed { get; set; }

    /// <summary>Quantidade de item por lote.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3)]
    public decimal? QtdItem { get; set; }

    /// <summary>Data de fabricação do medicamento (ddmmaaaa).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFab { get; set; }

    /// <summary>Data de expiração da validade do medicamento (ddmmaaaa).</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtVal { get; set; }

    /// <summary>Indicador de referência da base de cálculo do ICMS-ST do produto farmacêutico.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1)]
    public IndicadorBaseMedicamento? IndMed { get; set; }

    /// <summary>Tipo de produto farmacêutico.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1)]
    public TipoProdutoFarmaceutico? TpProd { get; set; }

    /// <summary>Valor do preço tabelado ou do preço máximo.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlTabMax { get; set; }
}
