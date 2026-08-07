using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y660 - dados das pessoas jurídicas sucessoras.</summary>
[RegistroSped(Codigo = "Y660", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY660 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y660";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ")]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NOM_EMP")]
    public string? NomEmp { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Decimais = 4, Nome = "PERC_PAT_LIQ")]
    public decimal? PercPatLiq { get; set; }
}
