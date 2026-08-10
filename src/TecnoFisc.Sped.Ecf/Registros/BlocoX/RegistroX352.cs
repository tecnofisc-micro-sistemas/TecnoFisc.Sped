using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X352 - resultados auferidos por intermédio de coligadas.</summary>
[RegistroSped(Codigo = "X352", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX352 : RegistroSped
{
    public override string Codigo => "X352";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_PER")]
    public decimal ResPer { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_PER_REAL")]
    public decimal ResPerReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_DISP")]
    public decimal LucDisp { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_DISP_REAL")]
    public decimal LucDispReal { get; set; }
}
