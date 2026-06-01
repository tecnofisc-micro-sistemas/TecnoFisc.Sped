using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K301 — Produção Conjunta — Industrialização Efetuada por Terceiros — Itens Produzidos.
/// Nível hierárquico 4, ocorrência vários por registro K300. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 266.
/// </summary>
[RegistroSped(Codigo = "K301", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK301 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K301";

    /// <summary>Código do item produzido (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade produzida.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal Qtd { get; set; }
}
