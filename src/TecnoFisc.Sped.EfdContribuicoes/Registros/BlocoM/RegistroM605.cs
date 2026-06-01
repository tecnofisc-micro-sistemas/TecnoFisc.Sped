using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M605 — Cofins a Recolher – Detalhamento por Código de Receita.
/// Nível hierárquico 3, ocorrência vários por arquivo. Filho de M600.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 342.
/// </summary>
[RegistroSped(Codigo = "M605", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM605 : RegistroSped
{
    public override string Codigo => "M605";

    /// <summary>Número do campo do Registro M600 objeto de detalhamento (08 ou 12).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NumCampo { get; set; }

    /// <summary>Código de receita referente à contribuição a recolher (6 dígitos, conforme DCTF).</summary>
    [CampoSped(Ordem = 3, Tamanho = 6, Obrigatorio = true)]
    public string? CodRec { get; set; }

    /// <summary>Valor do débito correspondente ao código do Campo 03.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDebito { get; set; }
}
