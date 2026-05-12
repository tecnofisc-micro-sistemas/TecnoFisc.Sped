using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C310 — Documentos Cancelados de Notas Fiscais de Venda a Consumidor (código 02).
/// Informa o número de cada documento fiscal cancelado dentro do intervalo do registro C300.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 108.
/// </summary>
[RegistroSped(Codigo = "C310", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C310";

    /// <summary>Número do documento fiscal cancelado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public int? NumDocCanc { get; set; }
}
