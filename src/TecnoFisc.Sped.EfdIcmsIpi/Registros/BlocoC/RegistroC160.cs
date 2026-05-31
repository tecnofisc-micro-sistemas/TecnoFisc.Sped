using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C160 — Complemento — Volumes Transportados (cód. 01 e 04) Exceto Combustíveis.
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 75.
/// </summary>
[RegistroSped(Codigo = "C160", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC160 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C160";

    /// <summary>Código do participante — transportador, se houver (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Placa de identificação do veículo automotor.</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public string? VeicId { get; set; }

    /// <summary>Quantidade de volumes transportados.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public int? QtdVol { get; set; }

    /// <summary>Peso bruto dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? PesoBrt { get; set; }

    /// <summary>Peso líquido dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? PesoLiq { get; set; }

    /// <summary>Sigla da UF da placa do veículo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2)]
    public string? UfId { get; set; }
}
