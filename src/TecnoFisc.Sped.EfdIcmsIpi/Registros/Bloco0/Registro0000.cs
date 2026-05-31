using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0000 — Abertura do Arquivo Digital e Identificação da Entidade.
/// Nível hierárquico 0, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 26.
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

    /// <inheritdoc />
    public override int VersaoLeiaute =>
        int.TryParse(CodVer, out var v) ? v : 0;

    /// <summary>Código da versão do leiaute conforme tabela indicada no Ato COTEPE (ex.: "015").</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CodVer { get; set; }

    /// <summary>Código da finalidade do arquivo: 0 = original, 1 = substituto.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorFinalidadeArquivo CodFin { get; set; }

    /// <summary>Data inicial das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das informações contidas no arquivo (ddMMyyyy).</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Nome empresarial da entidade.</summary>
    [CampoSped(Ordem = 6, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>Número de inscrição da entidade no CNPJ — mutuamente exclusivo com <see cref="Cpf"/>.</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>Número de inscrição da entidade no CPF — mutuamente exclusivo com <see cref="Cnpj"/>.</summary>
    [CampoSped(Ordem = 8, Tamanho = 11)]
    public Cpf? Cpf { get; set; }

    /// <summary>Sigla da unidade da federação da entidade (2 letras).</summary>
    [CampoSped(Ordem = 9, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Inscrição Estadual da entidade.</summary>
    [CampoSped(Ordem = 10, Tamanho = 14, Obrigatorio = true)]
    public InscricaoEstadual Ie { get; set; }

    /// <summary>Código do município do domicílio fiscal da entidade conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 11, Tamanho = 7, Obrigatorio = true)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição Municipal da entidade — preenchido quando houver.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? Im { get; set; }

    /// <summary>Inscrição da entidade na SUFRAMA — preenchido quando inscrita.</summary>
    [CampoSped(Ordem = 13, Tamanho = 9)]
    public string? Suframa { get; set; }

    /// <summary>Perfil de apresentação do arquivo fiscal: A, B ou C.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public string? IndPerfil { get; set; }

    /// <summary>Indicador do tipo de atividade: 0 = industrial/equiparado, 1 = outros.</summary>
    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true)]
    public IndicadorAtividadeIpi IndAtiv { get; set; }
}
