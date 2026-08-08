using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U182 - dados do cálculo da CSLL de empresas imunes e isentas.</summary>
[RegistroSped(Codigo = "U182", Nivel = 3, Bloco = "U")]
public sealed partial class RegistroU182 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U182";

    /// <summary>Código da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Valor textual da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
