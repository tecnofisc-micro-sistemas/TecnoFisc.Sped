using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N670 - apuração da CSLL com base no lucro real.</summary>
[RegistroSped(Codigo = "N670", Nivel = 3, Bloco = "N")]
public sealed partial class RegistroN670 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N670";

    /// <summary>Código da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor declarado da linha.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? Valor { get; set; }
}
