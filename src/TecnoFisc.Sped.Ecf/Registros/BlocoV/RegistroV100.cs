using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V100 - recurso em moeda estrangeira recebido de exportações.</summary>
[RegistroSped(Codigo = "V100", Nivel = 4, Bloco = "V")]
public sealed partial class RegistroV100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V100";

    /// <summary>Identificação da linha conforme a tabela dinâmica publicada pela RFB.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição opcional da linha.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1000)]
    public string? Descricao { get; set; }

    /// <summary>Valor opcional preservado na representação definida pela tabela dinâmica.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
