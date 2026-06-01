using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1370 - Bicos da Bomba. Nivel hierarquico 3, ocorrencia varios por Registro 1350.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 279.
/// </summary>
[RegistroSped(Codigo = "1370", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1370 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1370";

    /// <summary>Numero sequencial do bico ligado a bomba.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumBico { get; set; }

    /// <summary>Codigo do produto constante do registro 0200.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Tanque que armazena o combustivel.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? NumTanque { get; set; }
}
