using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1975 - GIAF 3 - Guia de Informacao e Apuracao de Incentivos Fiscais e
/// Financeiros: Importacao (saidas internas por faixa de aliquota).
/// Nivel hierarquico 3, ocorrencia 1:4. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 299-300.
/// </summary>
[RegistroSped(Codigo = "1975", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1975 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1975";

    /// <summary>Aliquota incidente sobre as importacoes-base.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal AliqImpBase { get; set; }

    /// <summary>Saidas incentivadas de PI.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G310 { get; set; }

    /// <summary>Importacoes-base para o credito presumido.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G311 { get; set; }

    /// <summary>Credito presumido nas saidas internas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal G312 { get; set; }
}
