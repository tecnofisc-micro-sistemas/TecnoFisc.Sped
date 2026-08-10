using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C053 - subcontas correlatas.</summary>
[RegistroSped(Codigo = "C053", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC053 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C053";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "COD_IDT")]
    public string? CodIdt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true, Nome = "COD_CNT_CORR")]
    public string? CodCntCorr { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true, Nome = "NAT_SUB_CNT")]
    public string? NatSubCnt { get; set; }
}
