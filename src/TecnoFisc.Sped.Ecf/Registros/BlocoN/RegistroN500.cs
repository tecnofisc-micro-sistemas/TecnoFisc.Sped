using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N500 - base de cálculo do IRPJ após compensações de prejuízos.</summary>
[RegistroSped(Codigo = "N500", Nivel = 3, Bloco = "N")]
public sealed partial class RegistroN500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N500";

    /// <summary>Código da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição da linha na tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 3, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Valor textual declarado para a linha dinâmica.</summary>
    [CampoSped(Ordem = 4, Nome = "VALOR")]
    public string? Valor { get; set; }
}
