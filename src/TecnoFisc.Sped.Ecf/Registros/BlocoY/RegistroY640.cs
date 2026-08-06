using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y640 - participações em consórcios de empresas.</summary>
[RegistroSped(Codigo = "Y640", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY640 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y640";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public CondicaoDeclaranteConsorcio CondDecl { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? VlCons { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 14, Obrigatorio = true)]
    public Cnpj CnpjLid { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDecl { get; set; }
}
