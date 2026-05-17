using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C177 — Complemento de Item: Outras Informações (cód. 01, 55).
/// Válido a partir de 01/01/2019. Nível hierárquico 4, ocorrência 1:1 (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 89.
/// </summary>
[RegistroSped(Codigo = "C177", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC177 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C177";

    /// <summary>Código da informação adicional conforme tabela SEFAZ (item 5.6 do guia).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodInfItem { get; set; }
}
