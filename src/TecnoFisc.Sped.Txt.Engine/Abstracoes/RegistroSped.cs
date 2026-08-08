using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Base abstrata de todos os registros SPED. Carrega apenas o vínculo hierárquico genérico
/// (Pai/Filhos); as colunas tipadas vivem nas subclasses concretas decoradas com
/// <see cref="Atributos.RegistroSpedAttribute"/> e <see cref="Atributos.CampoSpedAttribute"/>.
/// </summary>
public abstract class RegistroSped
{
    private readonly List<RegistroSped> _filhos = [];
    private List<ErroFormato>? _errosDeFormato;

    /// <summary>
    /// Erros de conversão de campo capturados em modo leniente (ver
    /// <see cref="Parser.ReadingOptions.LenientFieldParsing"/> e
    /// <see cref="Parser.LeitorSpedTxt.ParseLinha"/>).
    /// Vazia quando o registro foi lido sem problemas ou em modo estrito. O campo correspondente
    /// a cada erro permanece no valor default.
    /// </summary>
    public IReadOnlyList<ErroFormato> ErrosDeFormato => _errosDeFormato ?? (IReadOnlyList<ErroFormato>)[];

    internal void RegistrarErroDeFormato(ErroFormato erro) => (_errosDeFormato ??= []).Add(erro);

    /// <summary>Código do registro como aparece no arquivo SPED (ex.: "0000", "C100").</summary>
    public abstract string Codigo { get; }

    /// <summary>
    /// Versão do leiaute declarada no arquivo, extraída do campo <c>COD_VER</c> do
    /// <c>Registro0000</c>. Retorna <c>0</c> para todos os demais registros; o
    /// <c>Registro0000</c> de cada módulo faz override retornando o valor numérico real.
    /// Exposto ao consumidor e, quando o parser especializado habilita vigência sintática,
    /// usado para aplicar <c>IntroduzidoEm</c>/<c>DesdeVersao</c> (ARCHITECTURE §4.7).
    /// </summary>
    public virtual int VersaoLeiaute => 0;

    /// <summary>
    /// Indica se a versão declarada por este registro pertence à faixa de leiautes que o módulo
    /// modela. Só o registro de abertura (<c>0000</c>) de cada módulo tem essa informação; os
    /// demais herdam <c>true</c>, que preserva o comportamento estrito. Quando <c>false</c>, o
    /// leitor separa dois casos pelo valor de <see cref="VersaoLeiaute"/>: positivo significa
    /// leiaute novo ou antigo demais e o leitor degrada para diagnóstico em vez de exceção — um
    /// arquivo de leiaute que a biblioteca ainda não conhece deve ser legível, não fatal; zero
    /// significa versão ilegível no arquivo, e aí o leitor registra o diagnóstico mas <b>mantém o
    /// modo estrito</b>, porque dado corrompido não é evolução de leiaute.
    /// <para>
    /// Ressalva: o próprio <c>0000</c> é interpretado <b>antes</b> de o leitor conseguir
    /// consultar esta propriedade — ele precisa terminar de montar o registro para então ler
    /// <see cref="VersaoLeiaute"/> e <c>IsLeiauteConhecido</c>. Se um leiaute desconhecido
    /// mudar o formato de um campo do próprio <c>0000</c> (posição, tipo, domínio), a leitura
    /// desse registro específico ainda ocorre em modo estrito e pode abortar antes que a
    /// biblioteca saiba que está diante de um leiaute que não conhece. O modo tolerante só se
    /// aplica aos registros <b>seguintes</b> ao <c>0000</c>.
    /// </para>
    /// </summary>
    public virtual bool IsLeiauteConhecido => true;

    /// <summary>Registro pai na hierarquia, ou <c>null</c> se este é o raiz.</summary>
    public RegistroSped? Pai { get; internal set; }

    /// <summary>Filhos diretos na hierarquia, na ordem de aparição no arquivo.</summary>
    public IReadOnlyList<RegistroSped> Filhos => _filhos;

    internal void AdicionarFilho(RegistroSped filho)
    {
        filho.Pai = this;
        _filhos.Add(filho);
    }
}
