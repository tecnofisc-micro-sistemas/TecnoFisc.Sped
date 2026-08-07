using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C355 - detalhes dos saldos de resultado antes do encerramento.</summary>
[RegistroSped(Codigo = "C355", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C355";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_CTA")]
    public decimal VlCta { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_CTA")]
    public IndicadorDebitoCredito IndVlCta { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true, Nome = "LINHA_ECD")]
    public int LinhaEcd { get; set; }
}
