using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0002 — Classificação do Estabelecimento Industrial ou Equiparado a Industrial.
/// Nível hierárquico 2, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 27-28. Informado quando IND_ATIV do Registro0000 for igual a "0".
/// </summary>
[RegistroSped(Codigo = "0002", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0002 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0002";

    /// <summary>
    /// Classificação do estabelecimento conforme Tabela 4.5.5 — Classificação de Contribuintes
    /// do IPI (tabela externa hospedada em sped.rfb.gov.br).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int ClasEstabInd { get; set; }
}
