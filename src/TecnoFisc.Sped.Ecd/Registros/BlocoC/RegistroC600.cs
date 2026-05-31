using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C600 — Demonstrações Contábeis Recuperadas. Nível hierárquico 2, ocorrência 1:N.
/// Informa as demonstrações contábeis recuperadas da ECD anterior.
/// Campo(s) chave: [DT_INI]+[DT_FIN]+[ID_DEM].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 99.
/// </summary>
[RegistroSped(Codigo = "C600", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C600";

    /// <summary>Data inicial das demonstrações contábeis (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das demonstrações contábeis (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Identificação das demonstrações: 1 = empresário/sociedade; 2 = consolidadas/outras PJ.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IdentificacaoDemonstracao IdDem { get; set; }

    /// <summary>Cabeçalho das demonstrações (texto livre, máx. 65535 caracteres).</summary>
    [CampoSped(Ordem = 5, Tamanho = 65535)]
    public string? CabDem { get; set; }
}
