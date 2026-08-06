using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X400 - comércio eletrônico e tecnologia da informação.</summary>
[RegistroSped(Codigo = "X400", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX400 : RegistroSped
{
    public override string Codigo => "X400";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor de tabela dinâmica, preservado sem coerção.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
