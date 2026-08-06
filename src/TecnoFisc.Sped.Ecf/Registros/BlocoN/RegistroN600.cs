using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N600 - demonstração do lucro da exploração.</summary>
[RegistroSped(Codigo = "N600", Nivel = 3, Bloco = "N")]
public sealed partial class RegistroN600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N600";

    /// <summary>Código da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Valor textual declarado para a linha dinâmica.</summary>
    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
