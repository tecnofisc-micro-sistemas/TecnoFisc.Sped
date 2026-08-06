using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C051 - plano de contas referencial.</summary>
[RegistroSped(Codigo = "C051", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC051 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C051";

    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRef { get; set; }
}
