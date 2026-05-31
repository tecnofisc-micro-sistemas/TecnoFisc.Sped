using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

/// <summary>
/// Registro 9001 - Abertura do Bloco 9. Nivel hierarquico 1, ocorrencia unica por arquivo.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 302.
/// </summary>
[RegistroSped(Codigo = "9001", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9001";

    /// <summary>Indicador de movimento do bloco - 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
