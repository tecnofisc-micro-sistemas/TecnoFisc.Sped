using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K260 — Reprocessamento/Reparo de Produto/Insumo.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 258-259.
/// </summary>
[RegistroSped(Codigo = "K260", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK260 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K260";

    /// <summary>Código de identificação da ordem de produção ou de serviço.</summary>
    [CampoSped(Ordem = 2, Tamanho = 30)]
    public string? CodOpOs { get; set; }

    /// <summary>Código do produto/insumo a ser reprocessado/reparado (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Data de saída do estoque (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtSaida { get; set; }

    /// <summary>Quantidade de saída do estoque.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdSaida { get; set; }

    /// <summary>Data de retorno ao estoque (entrada), quando houver (ddMMyyyy).</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRet { get; set; }

    /// <summary>Quantidade de retorno ao estoque (entrada), quando houver retorno.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 6)]
    public decimal? QtdRet { get; set; }
}
