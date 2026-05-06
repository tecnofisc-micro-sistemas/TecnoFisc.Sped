using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M205 — PIS/Pasep a Recolher – Detalhamento por Código de Receita.
/// Nível hierárquico 3, ocorrência vários por arquivo. Filho de M200.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 308.
/// </summary>
[RegistroSped(Codigo = "M205", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM205 : RegistroSped
{
    public override string Codigo => "M205";

    /// <summary>Número do campo do Registro M200 objeto de detalhamento (08 ou 12).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NumCampo { get; set; }

    /// <summary>Código de receita referente à contribuição a recolher (6 dígitos, conforme DCTF).</summary>
    [CampoSped(Ordem = 3, Tamanho = 6, Obrigatorio = true)]
    public string? CodRec { get; set; }

    /// <summary>Valor do débito correspondente ao código do Campo 03.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDebito { get; set; }
}
