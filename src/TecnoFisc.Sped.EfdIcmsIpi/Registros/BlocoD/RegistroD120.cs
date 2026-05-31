using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D120 — Complemento da Nota Fiscal de Serviços de Transporte (Código 07).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 169.
/// </summary>
[RegistroSped(Codigo = "D120", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD120 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D120";

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 2, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>Código do município de destino do serviço, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Placa de identificação do veículo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 7)]
    public string? VeicId { get; set; }

    /// <summary>Sigla da UF da placa do veículo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2)]
    public string? UfId { get; set; }
}
