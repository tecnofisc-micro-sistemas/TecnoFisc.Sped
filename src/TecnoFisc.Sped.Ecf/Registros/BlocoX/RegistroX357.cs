using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X357 - investidoras diretas.</summary>
[RegistroSped(Codigo = "X357", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX357 : RegistroSped
{
    public override string Codigo => "X357";

    /// <summary>País conforme tabela dinâmica do Sped, preservado lexicalmente.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    /// <summary>NIF ou CNPJ composto, sem coerção para um identificador brasileiro.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NIF_CNPJ")]
    public string? NifCnpj { get; set; }

    [CampoSped(Ordem = 4, Obrigatorio = true)]
    public string? RazaoSocial { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal Percentual { get; set; }
}
