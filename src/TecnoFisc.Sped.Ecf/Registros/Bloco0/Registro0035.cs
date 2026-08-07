using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0035 — identificação das sociedades em conta de participação.</summary>
[RegistroSped(Codigo = "0035", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0035 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0035";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true, Nome = "COD_SCP")]
    public Cnpj CodScp { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "NOME_SCP")]
    public string? NomeScp { get; set; }
}
