using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K300 — Produção Conjunta — Industrialização Efetuada por Terceiros.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 265-266.
/// </summary>
[RegistroSped(Codigo = "K300", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K300";

    /// <summary>Data do reconhecimento da produção ocorrida no terceiro (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtProd { get; set; }
}
