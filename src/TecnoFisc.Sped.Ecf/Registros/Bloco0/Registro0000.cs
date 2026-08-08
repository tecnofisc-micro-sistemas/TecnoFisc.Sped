using System.Globalization;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>
/// Registro 0000 — abertura do arquivo digital e identificação da pessoa jurídica.
/// Leiaute 12, páginas 58–67 do manual da ECF.
/// </summary>
[RegistroSped(Codigo = "0000", Nivel = 0, Bloco = "0")]
public sealed partial class Registro0000 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0000";

    /// <summary>
    /// Versão declarada em <see cref="CodVer"/>, convertida em número. Devolve o valor mesmo
    /// fora da faixa que a biblioteca conhece (ver <see cref="LayoutEcf"/>): descartar o número
    /// desligaria o gate de vigência e faria o arquivo ser lido como se fosse leiaute 12. Um
    /// leiaute numérico fora da faixa conhecida (ex.: <c>0013</c>) é sinalizado à parte, por
    /// <see cref="IsLeiauteConhecido"/>, não por zero — e esse sinal chega ao leitor, porque
    /// <c>LeitorSpedTxt.ReadStreamingAsync</c> só consulta <see cref="IsLeiauteConhecido"/>
    /// quando <c>VersaoLeiaute &gt; 0</c>.
    /// <para>
    /// Zero é um caso diferente: <c>COD_VER</c> ausente, de comprimento diferente de 4 ou não
    /// numérico (ex.: <c>"ABCD"</c>) — arquivo inválido, não leiaute novo. Aqui
    /// <see cref="IsLeiauteConhecido"/> também é <c>false</c> (zero está fora de
    /// <see cref="LayoutEcf.V008"/>–<see cref="LayoutEcf.V012"/>), e o leitor distingue os dois
    /// casos: com versão positiva fora da faixa, avisa e passa a ler em modo tolerante; com versão
    /// zero, registra em <c>ErrosDeFormato</c> um erro apontando
    /// <c>COD_VER</c> como ilegível — dizendo que a vigência do leiaute não será aplicada — e
    /// <b>mantém o modo estrito</b>. Dado corrompido não autoriza afrouxar a leitura, mas também
    /// não passa em silêncio.
    /// </para>
    /// </summary>
    public override int VersaoLeiaute =>
        CodVer is { Length: 4 } &&
        int.TryParse(CodVer, NumberStyles.None, CultureInfo.InvariantCulture, out int versao)
            ? versao
            : 0;

    /// <inheritdoc />
    public override bool IsLeiauteConhecido =>
        VersaoLeiaute >= (int)LayoutEcf.V008 && VersaoLeiaute <= (int)LayoutEcf.V012;

    /// <summary>Identificador fixo do tipo de escrituração: <c>LECF</c>.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true, Nome = "NOME_ESC")]
    public string? NomeEsc { get; set; } = "LECF";

    /// <summary>
    /// Código declarado da versão do leiaute. A biblioteca modela <c>0008</c> a <c>0012</c>;
    /// um valor fora dessa faixa é lido pelo número declarado e sinalizado por
    /// <see cref="IsLeiauteConhecido"/>, que passa a <c>false</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true, Nome = "COD_VER")]
    public string? CodVer { get; set; }

    /// <summary>CNPJ do declarante ou do sócio ostensivo, no caso de SCP.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ")]
    public Cnpj Cnpj { get; set; }

    /// <summary>Nome empresarial da pessoa jurídica ou da SCP.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true, Nome = "NOME")]
    public string? Nome { get; set; }

    /// <summary>Indicador do início do período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true, Nome = "IND_SIT_INI_PER")]
    public int IndSitIniPer { get; set; }

    /// <summary>Indicador de situação especial ou outro evento.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true, Nome = "SIT_ESPECIAL")]
    public string? SitEspecial { get; set; }

    /// <summary>Percentual do patrimônio remanescente em caso de cisão.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4, Nome = "PAT_REMAN_CIS")]
    public decimal? PatRemanCis { get; set; }

    /// <summary>Data da situação especial ou do evento.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_SIT_ESP")]
    public DateOnly? DtSitEsp { get; set; }

    /// <summary>Data inicial das informações contidas no arquivo.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_INI")]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das informações contidas no arquivo.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_FIN")]
    public DateOnly DtFin { get; set; }

    /// <summary>Indicador de escrituração original, retificadora ou com mudança de tributação.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true, Nome = "RETIFICADORA")]
    public string? Retificadora { get; set; }

    /// <summary>Número do recibo da ECF anterior, quando aplicável.</summary>
    [CampoSped(Ordem = 13, Tamanho = 40, Nome = "NUM_REC")]
    public string? NumRec { get; set; }

    /// <summary>Indicador do tipo da ECF.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true, Nome = "TIP_ECF")]
    public int TipEcf { get; set; }

    /// <summary>CNPJ da SCP, preenchido somente pela própria SCP.</summary>
    [CampoSped(Ordem = 15, Tamanho = 14, Nome = "COD_SCP")]
    public Cnpj? CodScp { get; set; }
}
