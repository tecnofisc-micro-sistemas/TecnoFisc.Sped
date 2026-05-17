using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0400 — Tabela de Natureza da Operação/Prestação. Nível hierárquico 2,
/// ocorrência vários por arquivo. Codifica os textos das diferentes naturezas da
/// operação/prestação discriminadas nos documentos fiscais. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 41.
/// </summary>
[RegistroSped(Codigo = "0400", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0400 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0400";

    /// <summary>Código da natureza da operação/prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 10, Obrigatorio = true)]
    public string? CodNat { get; set; }

    /// <summary>Descrição da natureza da operação/prestação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? DescrNat { get; set; }
}
