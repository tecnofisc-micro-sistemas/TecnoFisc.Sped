using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco9;

/// <summary>
/// Registro 9900 — Registros do Arquivo.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Lista cada código de registro
/// presente no arquivo com a respectiva contagem de ocorrências, para verificação de
/// batimentos. Conforme Guia Prático EFD Contribuições v1.35, p. 415.
/// </summary>
[RegistroSped(Codigo = "9900", Nivel = 2, Bloco = "9")]
public sealed partial class Registro9900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9900";

    /// <summary>Código do registro cujas ocorrências são totalizadas.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? RegBlc { get; set; }

    /// <summary>Total de registros do tipo informado em REG_BLC.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public int QtdRegBlc { get; set; }
}
