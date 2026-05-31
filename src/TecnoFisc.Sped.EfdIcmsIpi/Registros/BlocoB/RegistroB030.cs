using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B030 — Nota Fiscal de Serviços Simplificada (Código 3A).
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 48.
/// </summary>
[RegistroSped(Codigo = "B030", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B030";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.3. Valor válido: 3A.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3)]
    public string? Ser { get; set; }

    /// <summary>Número do primeiro documento fiscal emitido no dia.</summary>
    [CampoSped(Ordem = 4, Tamanho = 9, Obrigatorio = true)]
    public int? NumDocIni { get; set; }

    /// <summary>Número do último documento fiscal emitido no dia.</summary>
    [CampoSped(Ordem = 5, Tamanho = 9, Obrigatorio = true)]
    public int? NumDocFin { get; set; }

    /// <summary>Data da emissão dos documentos fiscais no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Quantidade de documentos cancelados dentro da numeração entre NUM_DOC_INI e NUM_DOC_FIN.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int? QtdCanc { get; set; }

    /// <summary>Valor contábil (valor total acumulado dos documentos).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCont { get; set; }

    /// <summary>Valor acumulado das operações isentas ou não-tributadas pelo ISS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIsntIss { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ISS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIss { get; set; }

    /// <summary>Valor acumulado do ISS destacado.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIss { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 12, Tamanho = 60)]
    public string? CodInfObs { get; set; }
}
