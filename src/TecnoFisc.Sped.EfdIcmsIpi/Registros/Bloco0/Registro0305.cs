using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0305 — Informação sobre a Utilização do Bem. Nível hierárquico 3,
/// ocorrência 1:1 por Registro 0300. Obrigatório quando o campo IDENT_MERC do
/// Registro 0300 for igual a "1" (Bem). Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 40.
/// </summary>
[RegistroSped(Codigo = "0305", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0305 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0305";

    /// <summary>Código do centro de custo onde o bem está sendo ou será utilizado (campo 03 do Registro 0600).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição sucinta da função do bem na atividade do estabelecimento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Func { get; set; }

    /// <summary>Vida útil estimada do bem, em número de meses.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public int? VidaUtil { get; set; }
}
