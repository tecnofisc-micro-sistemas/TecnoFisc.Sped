using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X340 - identificação de participação no exterior.</summary>
[RegistroSped(Codigo = "X340", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX340 : RegistroSped
{
    public override string Codigo => "X340";

    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public string? RazSocial { get; set; }

    /// <summary>NIF lexical, inclusive marcadores 0000, 0001 e identificadores alfanuméricos.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public string? Nif { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public TipoControleExterior IndControle { get; set; }

    /// <summary>País conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 6, Obrigatorio = true)]
    public IndicadorSimNao IndIsenPetr { get; set; }

    [CampoSped(Ordem = 7, Obrigatorio = true)]
    public IndicadorSimNao IndConsol { get; set; }

    /// <summary>Motivo conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 8)]
    public string? MotNaoConsol { get; set; }

    /// <summary>CNPJ lexical que compõe a chave alternativa com o NIF.</summary>
    [CampoSped(Ordem = 9, Tamanho = 14)]
    public string? Cnpj { get; set; }

    /// <summary>Moeda conforme tabela dinâmica aplicável ao país.</summary>
    [CampoSped(Ordem = 10, Tamanho = 3, Obrigatorio = true)]
    public string? TipMoeda { get; set; }
}
