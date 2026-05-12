using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C300 — Resumo Diário das Notas Fiscais de Venda a Consumidor (código 02).
/// Agrega, por série e subsérie, todas as operações de venda a consumidor do dia
/// emitidas em formulário contínuo (não por ECF).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 103.
/// </summary>
[RegistroSped(Codigo = "C300", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C300";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1 (valor válido: 02).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public string? Sub { get; set; }

    /// <summary>Número do documento fiscal inicial do resumo diário.</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int? NumDocIni { get; set; }

    /// <summary>Número do documento fiscal final do resumo diário.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public int? NumDocFin { get; set; }

    /// <summary>Data da emissão dos documentos fiscais no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total dos documentos (excluídos os cancelados).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor total do PIS. Dispensado para contribuintes que entregam EFD-Contribuições.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS. Dispensado para contribuintes que entregam EFD-Contribuições.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0)]
    public string? CodCta { get; set; }
}
