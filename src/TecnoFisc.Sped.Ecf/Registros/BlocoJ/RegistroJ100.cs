using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J100 - centro de custos.</summary>
[RegistroSped(Codigo = "J100", Nivel = 2, Bloco = "J")]
public sealed partial class RegistroJ100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J100";

    /// <summary>Data de inclusão ou alteração do centro de custos.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_ALT")]
    public DateOnly DtAlt { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Nome do centro de custos.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true, Nome = "CCUS")]
    public string? Ccus { get; set; }
}
