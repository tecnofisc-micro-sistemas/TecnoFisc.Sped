using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0450 — Tabela de Informação Complementar do Documento Fiscal. Nível hierárquico 2,
/// ocorrência vários por arquivo. Codifica as informações complementares dos documentos fiscais
/// exigidas pela legislação fiscal, informadas no campo "Dados Adicionais". Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 41.
/// </summary>
[RegistroSped(Codigo = "0450", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0450 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0450";

    /// <summary>Código da informação complementar do documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodInf { get; set; }

    /// <summary>Texto livre da informação complementar existente no documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Txt { get; set; }
}
