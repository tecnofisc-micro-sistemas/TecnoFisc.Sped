using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D411 — Documentos Cancelados dos Documentos Informados (códigos 13, 14, 15 e 16).
/// Informa os números dos documentos fiscais cancelados contidos no intervalo do registro D410.
/// Nível hierárquico 4, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 198.
/// </summary>
[RegistroSped(Codigo = "D411", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD411 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D411";

    /// <summary>Número do documento fiscal cancelado (deve estar contido no intervalo NUM_DOC_INI–NUM_DOC_FIN do D410).</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public int? NumDocCanc { get; set; }
}
