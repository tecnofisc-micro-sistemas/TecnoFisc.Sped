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
    /// Exposto ao consumidor para que decida regras próprias por versão; em pacotes
    /// read-only o parser não usa este valor para filtrar registros (ARCHITECTURE §4.7).
    /// </summary>
    public virtual int VersaoLeiaute => 0;

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
