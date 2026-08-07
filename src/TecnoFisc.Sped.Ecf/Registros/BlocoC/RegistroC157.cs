using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C157 - transferência de saldos do plano anterior.</summary>
[RegistroSped(Codigo = "C157", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC157 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C157";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_FIN")]
    public decimal VlSldFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "IND_VL_SLD_FIN")]
    public IndicadorDebitoCredito? IndVlSldFin { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true, Nome = "LINHA_ECD")]
    public int LinhaEcd { get; set; }
}
