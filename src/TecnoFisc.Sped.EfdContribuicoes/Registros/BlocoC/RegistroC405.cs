using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C405 — Redução Z (Códigos 02 e 2D).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 154.
/// </summary>
[RegistroSped(Codigo = "C405", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC405 : RegistroSped
{
    public override string Codigo => "C405";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public int Cro { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public int Crz { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int NumCooFin { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal GtFin { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBrt { get; set; }
}
