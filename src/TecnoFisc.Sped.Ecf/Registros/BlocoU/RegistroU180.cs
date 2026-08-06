using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoU;

/// <summary>Registro U180 - dados do cálculo do IRPJ de empresas imunes e isentas.</summary>
[RegistroSped(Codigo = "U180", Nivel = 3, Bloco = "U")]
public sealed partial class RegistroU180 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "U180";

    /// <summary>Código da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor textual da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
