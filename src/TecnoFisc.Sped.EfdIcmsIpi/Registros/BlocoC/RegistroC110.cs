using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C110 — Informação Complementar da Nota Fiscal (Código 01, 1B, 04 e 55).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 67.
/// </summary>
[RegistroSped(Codigo = "C110", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C110";

    /// <summary>Código da informação complementar do documento fiscal (campo 02 do Registro 0450).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodInf { get; set; }

    /// <summary>Descrição complementar do código de referência.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}
