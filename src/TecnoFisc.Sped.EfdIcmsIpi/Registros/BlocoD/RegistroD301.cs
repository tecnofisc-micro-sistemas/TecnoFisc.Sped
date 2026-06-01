using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D301 — Documentos Cancelados dos Bilhetes de Passagem Rodoviário (13),
/// Aquaviário (14), Passagem e Nota de Bagagem (15) e Ferroviário (16).
/// Informa os números dos documentos fiscais cancelados no intervalo constante no registro pai (D300).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 183.
/// </summary>
[RegistroSped(Codigo = "D301", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD301 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D301";

    /// <summary>Número do documento fiscal cancelado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public int? NumDocCanc { get; set; }
}
