using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P100 — Contribuição Previdenciária sobre a Receita Bruta (CPRB). Nível hierárquico 3,
/// ocorrência 1:N (filho de P010). Um registro por código de atividade econômica (COD_ATIV_ECON)
/// da Tabela 5.1.1. Conforme Guia Prático v1.35, p. 367.
/// </summary>
[RegistroSped(Codigo = "P100", Nivel = 3, Bloco = "P")]
public sealed partial class RegistroP100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P100";

    /// <summary>Data inicial a que a apuração se refere.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtIni { get; set; }

    /// <summary>Data final a que a apuração se refere.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtFin { get; set; }

    /// <summary>Valor da Receita Bruta Total do Estabelecimento no Período.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecTotEst { get; set; }

    /// <summary>
    /// Código indicador correspondente à atividade sujeita à incidência da CPRB, conforme
    /// Tabela 5.1.1 (8 caracteres fixos).
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Obrigatorio = true)]
    public string? CodAtivEcon { get; set; }

    /// <summary>
    /// Valor da Receita Bruta do Estabelecimento correspondente às atividades/produtos
    /// referidos no Campo COD_ATIV_ECON.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecAtivEstab { get; set; }

    /// <summary>Valor das Exclusões da Receita Bruta informada no Campo VL_REC_ATIV_ESTAB.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlExc { get; set; }

    /// <summary>Valor da Base de Cálculo da CPRB (Campo VL_REC_ATIV_ESTAB – Campo VL_EXC).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCont { get; set; }

    /// <summary>Alíquota da CPRB sobre a Receita Bruta, conforme Tabela 5.1.1 (4 casas decimais).</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCont { get; set; }

    /// <summary>Valor da CPRB Apurada sobre a Receita Bruta (Campo VL_BC_CONT × Campo ALIQ_CONT).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContApu { get; set; }

    /// <summary>Código da conta analítica contábil referente à CPRB sobre a Receita Bruta.</summary>
    [CampoSped(Ordem = 11, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Informação complementar do registro.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
