using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco9;

/// <summary>Registro 9900 - totalização dos registros do arquivo por tipo.</summary>
[RegistroSped(Codigo = "9900", Nivel = 2, Bloco = "9")]
public sealed partial class Registro9900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9900";

    /// <summary>Código do registro totalizado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? RegBlc { get; set; }

    /// <summary>Quantidade de ocorrências do registro totalizado.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public int QtdRegBlc { get; set; }

    /// <summary>Versão da tabela dinâmica, quando aplicável.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Versao { get; set; }

    /// <summary>Identificação da tabela dinâmica, quando aplicável.</summary>
    [CampoSped(Ordem = 5)]
    public string? IdTabDin { get; set; }
}
