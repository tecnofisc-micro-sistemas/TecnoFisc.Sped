using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1360 - Lacres da Bomba. Nivel hierarquico 3, ocorrencia varios por Registro 1350.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 279.
/// </summary>
[RegistroSped(Codigo = "1360", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1360 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1360";

    /// <summary>Numero do lacre associado na bomba.</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumLacre { get; set; }

    /// <summary>Data de aplicacao do lacre.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAplicacao { get; set; }
}
