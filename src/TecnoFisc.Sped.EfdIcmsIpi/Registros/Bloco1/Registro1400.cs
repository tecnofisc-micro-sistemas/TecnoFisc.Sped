using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1400 - Informacao sobre Valores Agregados. Nivel hierarquico 2,
/// ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 282.
/// </summary>
[RegistroSped(Codigo = "1400", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1400 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1400";

    /// <summary>Codigo do item para indice de participacao dos municipios.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemIpm { get; set; }

    /// <summary>Codigo do municipio de origem ou destino, conforme tabela IBGE.</summary>
    [CampoSped(Ordem = 3, Tamanho = 7, Obrigatorio = true)]
    public int Mun { get; set; }

    /// <summary>Valor mensal correspondente ao municipio.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal Valor { get; set; }
}
