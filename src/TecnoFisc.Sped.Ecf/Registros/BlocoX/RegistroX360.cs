using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X360 - informações gerais sobre preços de transferência.</summary>
[RegistroSped(Codigo = "X360", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX360 : RegistroSped
{
    public override string Codigo => "X360";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor de tabela dinâmica, preservado sem coerção.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
