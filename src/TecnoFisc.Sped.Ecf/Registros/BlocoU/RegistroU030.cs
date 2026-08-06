using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U030 - período de apuração de empresas imunes e isentas.</summary>
[RegistroSped(Codigo = "U030", Nivel = 2, Bloco = "U")]
public sealed partial class RegistroU030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U030";

    /// <summary>Data inicial do período de apuração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final do período de apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Código do período, preservado sem validação tributária condicional.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? PerApur { get; set; }
}
