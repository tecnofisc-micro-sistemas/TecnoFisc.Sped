using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0000 — Abertura do Arquivo Digital e Identificação da Pessoa Jurídica.
/// Nível hierárquico 0, ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 66.
/// </summary>
/// <remarks>
/// O <c>partial</c> deixa o source generator do Stage 6 estender a classe com o catálogo
/// gerado em tempo de compilação; até lá o catálogo é montado por reflexão cacheada.
/// </remarks>
[RegistroSped(Codigo = "0000", Nivel = 0, Bloco = "0")]
public sealed partial class Registro0000 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0000";

    /// <summary>Código da versão do leiaute conforme tabela 3.1.1 do Guia Prático (ex.: "006").</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodVer { get; set; }

    /// <summary>Tipo de escrituração: original (0) ou retificadora (1).</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public TipoEscrituracao TipoEscrit { get; set; }

    /// <summary>Indicador de situação especial — preenchido apenas em eventos de abertura/cisão/fusão/incorporação/encerramento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorSituacaoEspecial? IndSitEsp { get; set; }

    /// <summary>Número do recibo da escrituração anterior — obrigatório quando <see cref="TipoEscrit"/> = Retificadora.</summary>
    [CampoSped(Ordem = 5, Tamanho = 41)]
    public string? NumRecAnterior { get; set; }

    /// <summary>Data inicial das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Nome empresarial da pessoa jurídica titular da escrituração (sem acentos).</summary>
    [CampoSped(Ordem = 8, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>CNPJ do estabelecimento matriz da pessoa jurídica.</summary>
    [CampoSped(Ordem = 9, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Sigla da Unidade da Federação do estabelecimento sede (2 letras).</summary>
    [CampoSped(Ordem = 10, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Código do município conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 11, Tamanho = 7, Obrigatorio = true)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição da pessoa jurídica na Suframa, em branco quando não inscrita.</summary>
    [CampoSped(Ordem = 12, Tamanho = 9)]
    public string? Suframa { get; set; }

    /// <summary>Indicador da natureza da pessoa jurídica (00..05).</summary>
    [CampoSped(Ordem = 13, Tamanho = 2)]
    public IndicadorNaturezaPessoaJuridica? IndNatPJ { get; set; }

    /// <summary>Indicador do tipo de atividade preponderante (0,1,2,3,4,9).</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public IndicadorAtividade IndAtiv { get; set; }
}
