using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y750 - informações da ECF calculadas pelo PGE.</summary>
[RegistroSped(Codigo = "Y750", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY750 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y750";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
