using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1350 - Bombas. Nivel hierarquico 2, ocorrencia varios por arquivo.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 278-279.
/// </summary>
[RegistroSped(Codigo = "1350", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1350";

    /// <summary>Numero de serie da bomba.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? Serie { get; set; }

    /// <summary>Nome do fabricante da bomba.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? Fabricante { get; set; }

    /// <summary>Modelo da bomba.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? Modelo { get; set; }

    /// <summary>Identificador do tipo de medicao da bomba.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public TipoMedicaoBomba TipoMedicao { get; set; }
}
