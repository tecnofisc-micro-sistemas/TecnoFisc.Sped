using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0190 — Identificação das Unidades de Medida. Nível hierárquico 2,
/// ocorrência vários por arquivo. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 32.
/// </summary>
[RegistroSped(Codigo = "0190", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0190 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0190";

    /// <summary>Código da unidade de medida. Deve existir em pelo menos um outro registro do arquivo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Descrição da unidade de medida.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Descr { get; set; }
}
