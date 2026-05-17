using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K250 — Industrialização Efetuada por Terceiros — Itens Produzidos.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 256-257.
/// </summary>
[RegistroSped(Codigo = "K250", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK250 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K250";

    /// <summary>Data do reconhecimento da produção ocorrida no terceiro (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtProd { get; set; }

    /// <summary>Código do item produzido (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade produzida.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal Qtd { get; set; }
}
