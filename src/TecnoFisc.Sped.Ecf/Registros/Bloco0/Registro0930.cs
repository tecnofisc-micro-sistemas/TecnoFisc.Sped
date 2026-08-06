using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0930 — identificação dos signatários da ECF.</summary>
[RegistroSped(Codigo = "0930", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0930 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0930";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? IdentNom { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 14, Obrigatorio = true)]
    public string? IdentCpfCnpj { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? IdentQualif { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? IndCrc { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 60, Obrigatorio = true)]
    public string? Email { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 14, Obrigatorio = true)]
    public string? Fone { get; set; }
}
