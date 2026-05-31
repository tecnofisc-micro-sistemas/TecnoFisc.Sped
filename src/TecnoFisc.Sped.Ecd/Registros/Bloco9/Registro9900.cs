using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco9;

/// <summary>
/// Registro 9900 — Registros do Arquivo. Nível hierárquico 2, ocorrência 1:N por arquivo.
/// Totaliza cada tipo de registro presente no arquivo ECD, inclusive os posteriores ao 9900.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 231.
/// </summary>
[RegistroSped(Codigo = "9900", Nivel = 2, Bloco = "9")]
public sealed partial class Registro9900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9900";

    /// <summary>Código do registro que será totalizado no campo <c>QTD_REG_BLC</c>.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? RegBlc { get; set; }

    /// <summary>Total de registros do tipo informado no campo <c>REG_BLC</c>.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public int QtdRegBlc { get; set; }
}
