using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y800 - documento RTF anexado à escrituração.</summary>
[RegistroSped(Codigo = "Y800", Nivel = 2, Bloco = "Y", TokenFimArquivo = "Y800FIM")]
public sealed partial class RegistroY800 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y800";

    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public TipoDocumentoY800 TipoDoc { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public string? Descricao { get; set; }

    /// <summary>Hash calculado pelo PGE; permanece vazio no arquivo de importação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 41, Obrigatorio = true)]
    public string? Hash { get; set; }

    [CampoSped(Ordem = 5, Obrigatorio = true, CampoArquivo = true)]
    public string? ArqRtf { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 7, Obrigatorio = true)]
    public string? IndFimRtf { get; set; }
}
