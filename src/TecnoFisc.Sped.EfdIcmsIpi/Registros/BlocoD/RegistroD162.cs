using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D162 — Identificação dos Documentos Fiscais (Códigos 08, 8B, 09, 10, 11, 26 e 27).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 175.
/// </summary>
[RegistroSped(Codigo = "D162", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD162 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D162";

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1. Valores válidos: 01, 1B, 04 e 55.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 9)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlDoc { get; set; }

    /// <summary>Valor das mercadorias constantes no documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlMerc { get; set; }

    /// <summary>Quantidade de volumes transportados.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public int? QtdVol { get; set; }

    /// <summary>Peso bruto dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? PesoBrt { get; set; }

    /// <summary>Peso líquido dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? PesoLiq { get; set; }
}
