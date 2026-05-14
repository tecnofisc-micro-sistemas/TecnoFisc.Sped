using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K255 — Industrialização em Terceiros — Insumos Consumidos.
/// Nível hierárquico 4, ocorrência vários por registro K250. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 257-258.
/// </summary>
[RegistroSped(Codigo = "K255", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK255 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K255";

    /// <summary>Data do reconhecimento do consumo do insumo referente ao produto (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtCons { get; set; }

    /// <summary>Código do insumo (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade de consumo do insumo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Código do insumo substituído, caso ocorra substituição (campo 02 do Registro 0210).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodInsSubst { get; set; }
}
