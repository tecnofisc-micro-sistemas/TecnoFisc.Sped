using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y720 - informações de períodos anteriores.</summary>
[RegistroSped(Codigo = "Y720", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY720 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y720";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_LIQ")]
    public decimal LucLiq { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_LUC_LIQ")]
    public DateOnly DtLucLiq { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_BRUT_ANT")]
    public decimal RecBrutAnt { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "INTIMACAO")]
    public IndicadorSimNao Intimacao { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Nome = "INT_ATRASO")]
    public IndicadorSimNao? IntAtraso { get; set; }
}
