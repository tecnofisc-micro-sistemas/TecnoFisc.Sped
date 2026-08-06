using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0030 — dados cadastrais.</summary>
[RegistroSped(Codigo = "0030", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0030";

    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? CodNat { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 7, Obrigatorio = true)]
    public string? CnaeFiscal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 150, Obrigatorio = true)]
    public string? Endereco { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? Num { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 50)]
    public string? Compl { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 50, Obrigatorio = true)]
    public string? Bairro { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 7, Obrigatorio = true)]
    public string? CodMun { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Obrigatorio = true)]
    public string? Cep { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 15)]
    public string? NumTel { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 115, Obrigatorio = true)]
    public string? Email { get; set; }
}
