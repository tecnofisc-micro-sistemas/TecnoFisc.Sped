using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X450 - pagamentos ou remessas por país.</summary>
[RegistroSped(Codigo = "X450", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX450 : RegistroSped
{
    public override string Codigo => "X450";

    /// <summary>País conforme tabela dinâmica, preservado como código lexical.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }
}
