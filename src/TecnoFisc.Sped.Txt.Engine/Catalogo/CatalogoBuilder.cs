using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Txt.Engine.Catalogo;

/// <summary>
/// Constrói catálogos via reflexão a partir de um <see cref="Assembly"/>. É a estratégia
/// fallback descrita no Stage 2 da arquitetura: reflexão paga uma vez na primeira chamada
/// e fica em cache; o resultado se compara em performance ao catálogo gerado em tempo de
/// compilação (Stage 6) porque os setters/conversores são compilados em delegates.
/// </summary>
public static class CatalogoBuilder
{
    private static readonly ConcurrentDictionary<Assembly, IRegistroSpedCatalogo> _cache = new();

    /// <summary>
    /// Varre o assembly procurando classes decoradas com <see cref="RegistroSpedAttribute"/>
    /// e devolve um catálogo pronto para uso. O resultado é memoizado por assembly.
    /// </summary>
    public static IRegistroSpedCatalogo BuildFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return _cache.GetOrAdd(assembly, ConstruirNovo);
    }

    /// <summary>Limpa o cache do builder. Útil em cenários de teste.</summary>
    public static void LimparCache() => _cache.Clear();

    /// <summary>
    /// Constrói os metadados de um único registro a partir do tipo e da fábrica fornecida.
    /// Usado pelo catálogo gerado em tempo de compilação (Stage 6) — o gerador conhece o tipo
    /// e a fábrica em compile-time, mas reusa este helper para extrair via reflexão a tabela
    /// de campos uma única vez na inicialização do catálogo. Reflexão fica fora da hot path
    /// de parsing porque os <see cref="MetadadosCampo"/> internos guardam delegates compilados.
    /// </summary>
    public static MetadadosRegistro BuildMetadataForType(
        Type tipo,
        string codigo,
        int nivel,
        string bloco,
        Func<RegistroSped> fabrica,
        int introduzidoEm = 0,
        string? tokenFimArquivo = null)
    {
        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(bloco);
        ArgumentNullException.ThrowIfNull(fabrica);

        if (tipo.IsAbstract || !typeof(RegistroSped).IsAssignableFrom(tipo))
            throw new InvalidOperationException(
                $"Tipo {tipo.FullName} precisa ser concreto e herdar de RegistroSped.");

        var campos = ConstruirCampos(tipo);
        tokenFimArquivo ??= tipo.GetCustomAttribute<RegistroSpedAttribute>(inherit: false)?.TokenFimArquivo;
        ValidarCampoArquivo(tipo, campos, tokenFimArquivo);
        return new MetadadosRegistro(
            codigo, nivel, bloco, tipo, fabrica, campos, introduzidoEm,
            descontinuadoEm: 0, tokenFimArquivo: tokenFimArquivo);
    }

    private static IRegistroSpedCatalogo ConstruirNovo(Assembly assembly)
    {
        var registros = new Dictionary<string, MetadadosRegistro>(StringComparer.Ordinal);

        foreach (var tipo in assembly.GetTypes())
        {
            var atributo = tipo.GetCustomAttribute<RegistroSpedAttribute>(inherit: false);
            if (atributo is null)
                continue;

            if (tipo.IsAbstract || !typeof(RegistroSped).IsAssignableFrom(tipo))
                throw new InvalidOperationException(
                    $"Tipo {tipo.FullName} marcado com [RegistroSped] precisa ser concreto e herdar de RegistroSped.");

            var metadados = ConstruirMetadados(tipo, atributo);

            if (!registros.TryAdd(metadados.Codigo, metadados))
                throw new InvalidOperationException(
                    $"Código de registro duplicado '{metadados.Codigo}' em {assembly.GetName().Name}: " +
                    $"{registros[metadados.Codigo].TipoCSharp.FullName} e {tipo.FullName}.");
        }

        return new CatalogoReflexivo(registros);
    }

    private static MetadadosRegistro ConstruirMetadados(Type tipo, RegistroSpedAttribute atributo)
    {
        var fabrica = ConstruirFabrica(tipo);
        var campos = ConstruirCampos(tipo);
        ValidarCampoArquivo(tipo, campos, atributo.TokenFimArquivo);
        var descontinuado = tipo.GetCustomAttribute<DescontinuadoAttribute>(inherit: false);
        return new MetadadosRegistro(
            atributo.Codigo,
            atributo.Nivel,
            atributo.Bloco,
            tipo,
            fabrica,
            campos,
            atributo.IntroduzidoEm,
            descontinuado?.EmVersao ?? 0,
            atributo.TokenFimArquivo);
    }

    /// <summary>
    /// Valida a consistência de um registro com campo-arquivo embutido (J800/J801 e similares).
    /// As regras refletem o que o reassembly multi-linha do <c>LeitorSpedTxt</c> consegue
    /// interpretar — uma única âncora de fim conhecida é necessária porque o campo-arquivo pode
    /// conter <c>|</c> e CRLF:
    /// <list type="number">
    ///   <item>no máximo um campo marcado com <c>CampoArquivo</c>;</item>
    ///   <item><c>CampoArquivo</c> e <c>TokenFimArquivo</c> andam em par (um exige o outro);</item>
    ///   <item>o campo-arquivo é do tipo <c>string</c>;</item>
    ///   <item>o campo-arquivo é o penúltimo campo (seguido apenas pelo terminador IND_FIM_*).</item>
    /// </list>
    /// Se um leiaute futuro (ex.: ECF) trouxer outra forma, esta validação falha apontando o
    /// registro — sinal de que o reassembly precisa ser generalizado (com teste), em vez de gerar
    /// dado errado calado.
    /// </summary>
    private static void ValidarCampoArquivo(Type tipo, IReadOnlyList<MetadadosCampo> campos, string? tokenFimArquivo)
    {
        int indice = -1;
        for (int i = 0; i < campos.Count; i++)
        {
            if (!campos[i].CampoArquivo)
                continue;
            if (indice >= 0)
                throw new InvalidOperationException(
                    $"{tipo.FullName} tem mais de um campo com CampoArquivo; só é permitido um por registro.");
            indice = i;
        }

        bool temCampoArquivo = indice >= 0;
        bool temToken = tokenFimArquivo is not null;

        if (temCampoArquivo != temToken)
            throw new InvalidOperationException(
                $"{tipo.FullName}: CampoSped.CampoArquivo e RegistroSped.TokenFimArquivo devem ser usados " +
                $"em par (campoArquivo={(temCampoArquivo ? "presente" : "ausente")}, " +
                $"TokenFimArquivo={(temToken ? $"\"{tokenFimArquivo}\"" : "ausente")}).");

        if (!temCampoArquivo)
            return;

        var campo = campos[indice];
        if (campo.Tipo != typeof(string))
            throw new InvalidOperationException(
                $"CampoArquivo em {tipo.FullName}.{campo.Nome} requer tipo string (nullable ou não).");

        if (indice != campos.Count - 2)
            throw new InvalidOperationException(
                $"CampoArquivo em {tipo.FullName}.{campo.Nome} deve ser o penúltimo campo " +
                $"(seguido apenas pelo campo terminador IND_FIM_*); encontrado na posição {indice + 2} " +
                $"de {campos.Count + 1} (REG = 1).");
    }

    private static Func<RegistroSped> ConstruirFabrica(Type tipo)
    {
        var ctor = tipo.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (ctor is null)
            throw new InvalidOperationException(
                $"Tipo {tipo.FullName} precisa de um construtor sem parâmetros para servir de registro SPED.");

        var corpo = Expression.Convert(Expression.New(ctor), typeof(RegistroSped));
        return Expression.Lambda<Func<RegistroSped>>(corpo).Compile();
    }

    private static IReadOnlyList<MetadadosCampo> ConstruirCampos(Type tipo)
    {
        var lista = new List<(int Ordem, MetadadosCampo Campo)>();
        var ordensVistas = new HashSet<int>();
        var fieldNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var atributo = propriedade.GetCustomAttribute<CampoSpedAttribute>(inherit: true);
            if (atributo is null)
                continue;

            if (!ordensVistas.Add(atributo.Ordem))
                throw new InvalidOperationException(
                    $"Ordem duplicada {atributo.Ordem} em {tipo.FullName}.{propriedade.Name}.");

            string fieldName = ResolveFieldName(tipo, propriedade, atributo);
            if (!fieldNames.Add(fieldName))
                throw new InvalidOperationException(
                    $"Nome de campo duplicado '{fieldName}' em {tipo.FullName}.{propriedade.Name}.");

            if (propriedade.SetMethod is null)
                throw new InvalidOperationException(
                    $"Propriedade {tipo.FullName}.{propriedade.Name} precisa de setter para receber valor SPED.");

            // Compõe a nova API zero-alloc-no-chamador a partir dos delegates intermediários:
            // o caminho reflexivo continua boxando (string + object) por dentro, mas o consumidor
            // só vê os dois delegates da API pública de MetadadosCampo. O definidor padrão é
            // sempre o permissivo (validarDominio: false) — preserva o comportamento já publicado
            // nos três pacotes. O estrito só é construído quando há política de domínio a aplicar
            // (enum fechado sem [SpedValor]); os demais campos recebem null e a leitura em tempo
            // de parsing cai de volta no permissivo (ver MetadadosCampo.Definidor).
            Action<RegistroSped, ReadOnlySpan<char>> definidor =
                ComporDefinidor(propriedade, atributo, validarDominio: false);
            Action<RegistroSped, ReadOnlySpan<char>>? definidorEstrito =
                RequiresStrictSetter(propriedade.PropertyType, atributo)
                    ? ComporDefinidor(propriedade, atributo, validarDominio: true)
                    : null;

            var getter = ConstruirGetter(propriedade);
            var serializador = ConstruirSerializador(propriedade.PropertyType, atributo);

            Func<RegistroSped, string> serializadorComposto = registro =>
            {
                object? leitura = getter(registro);
                return leitura is null ? string.Empty : serializador(leitura);
            };

            lista.Add((atributo.Ordem, new MetadadosCampo(
                fieldName,
                atributo.Ordem,
                propriedade.PropertyType,
                atributo.Tamanho,
                atributo.Decimais,
                atributo.Obrigatorio,
                atributo.Formato,
                definidor,
                serializadorComposto,
                atributo.DesdeVersao,
                atributo.CapturaTudo,
                atributo.CampoArquivo,
                definidorEstrito)));
        }

        lista.Sort(static (a, b) => a.Ordem.CompareTo(b.Ordem));

        // Detecta lacunas — REG é Nº 1 (implícito); os campos marcados começam em 2 e
        // devem ser sequenciais, casando com a coluna "Nº" do Guia Prático.
        for (int i = 0; i < lista.Count; i++)
        {
            int esperado = i + 2;
            if (lista[i].Ordem != esperado)
                throw new InvalidOperationException(
                    $"Ordens de campos de {tipo.FullName} devem ser sequenciais a partir de 2 " +
                    $"(REG ocupa a posição 1); esperava {esperado}, encontrei {lista[i].Ordem} ({lista[i].Campo.Nome}).");
        }

        // CapturaTudo só é permitido no último campo e apenas para string?
        for (int i = 0; i < lista.Count; i++)
        {
            var (_, campo) = lista[i];
            if (!campo.CapturaTudo)
                continue;
            if (i != lista.Count - 1)
                throw new InvalidOperationException(
                    $"CapturaTudo em {tipo.FullName}.{campo.Nome} só é permitido no último campo do registro.");
            if (campo.Tipo != typeof(string))
                throw new InvalidOperationException(
                    $"CapturaTudo em {tipo.FullName}.{campo.Nome} requer tipo string (nullable ou não).");
        }

        ValidarVigenciaCrescente(tipo, lista);

        return lista.Count == 0
            ? Array.Empty<MetadadosCampo>()
            : lista.ConvertAll(static x => x.Campo);
    }

    /// <summary>
    /// Exige que <see cref="CampoSpedAttribute.DesdeVersao"/> seja não-decrescente ao longo da
    /// posição dos campos (tratando <c>0</c> — "sempre presente" — como o mínimo possível). É a
    /// invariante da qual o mapeamento posicional do leitor depende
    /// (<c>LeitorSpedTxt.InterpretarLinha</c>, <c>indice = posicaoCampo - 2</c>): um campo
    /// versionado só pode aparecer no <b>fim</b> do registro, nunca seguido por um campo sempre
    /// presente ou por um campo de versão anterior. Um arquivo SPED real nunca reordena colunas —
    /// o Guia Prático só acrescenta campos novos ao final numa revisão — então uma sequência
    /// decrescente indicaria erro de modelagem, não um leiaute real. Espelha
    /// <c>RegistroSpedCatalogoGenerator</c> (diagnóstico em tempo de compilação), para que o
    /// catálogo reflexivo e o gerado concordem sobre o que é um registro válido.
    /// </summary>
    private static void ValidarVigenciaCrescente(Type tipo, List<(int Ordem, MetadadosCampo Campo)> lista)
    {
        for (int i = 1; i < lista.Count; i++)
        {
            var anterior = lista[i - 1].Campo;
            var atual = lista[i].Campo;
            if (atual.DesdeVersao < anterior.DesdeVersao)
                throw new InvalidOperationException(
                    $"Campo {tipo.FullName}.{atual.Nome} na posição {lista[i].Ordem} " +
                    $"(DesdeVersao={atual.DesdeVersao}) vem depois de {anterior.Nome} na posição " +
                    $"{lista[i - 1].Ordem} (DesdeVersao={anterior.DesdeVersao}); DesdeVersao precisa " +
                    "ser não-decrescente ao longo da posição dos campos — campo versionado só pode " +
                    "ficar no fim do registro, senão o mapeamento posicional do leitor desalinha " +
                    "silenciosamente. Mova o campo para o fim do registro ou remova DesdeVersao.");
        }
    }

    /// <summary>
    /// Divergência intencional vs. <c>RegistroSpedCatalogoGenerator</c> (compile-time): aqui não
    /// existe canal de diagnóstico, então lançar é o próprio diagnóstico — nenhum catálogo é
    /// construído para o tipo com alias inválido. O gerador, que tem <c>TFSPED001</c> como
    /// diagnóstico de build, reporta o erro e cai para o nome CLR da propriedade em vez de
    /// lançar, para não suprimir o resto do catálogo gerado do assembly (achado 6 do PR 531). Os
    /// dois caminhos só produzem resultado diferente para o mesmo tipo se <c>TFSPED001</c> for
    /// suprimido via <c>#pragma</c>/<c>.editorconfig</c> — nesse cenário artificial o catálogo
    /// gerado teria o nome CLR onde este método continuaria lançando.
    /// </summary>
    private static string ResolveFieldName(
        Type type,
        PropertyInfo property,
        CampoSpedAttribute attribute)
    {
        string? alias = attribute.Nome;
        if (string.IsNullOrEmpty(alias))
            return property.Name;

        if (!IsFieldNameValid(alias))
        {
            throw new InvalidOperationException(
                $"Nome de campo SPED inválido '{EscapeFieldName(alias)}' em " +
                $"{type.FullName}.{property.Name}.");
        }

        return alias;
    }

    private static bool IsFieldNameValid(string name)
    {
        if (name.Length == 0 || !IsFieldNameStart(name[0]))
            return false;

        for (int i = 1; i < name.Length; i++)
        {
            char character = name[i];
            if (!IsFieldNameStart(character) && (character < '0' || character > '9'))
                return false;
        }

        return true;
    }

    private static bool IsFieldNameStart(char character)
        => character == '_' ||
           (character >= 'A' && character <= 'Z') ||
           (character >= 'a' && character <= 'z') ||
           (character >= '\u00C0' && character <= '\u00FF' && char.IsLetter(character));

    private static string EscapeFieldName(string name)
        => name
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private static Action<RegistroSped, object?> ConstruirSetter(PropertyInfo propriedade)
    {
        var paramRegistro = Expression.Parameter(typeof(RegistroSped), "registro");
        var paramValor = Expression.Parameter(typeof(object), "valor");

        Expression registroTipado = Expression.Convert(paramRegistro, propriedade.DeclaringType!);
        Expression valorTipado = Expression.Convert(paramValor, propriedade.PropertyType);

        var atribuicao = Expression.Assign(
            Expression.Property(registroTipado, propriedade),
            valorTipado);

        return Expression.Lambda<Action<RegistroSped, object?>>(
            atribuicao, paramRegistro, paramValor).Compile();
    }

    private static Func<RegistroSped, object?> ConstruirGetter(PropertyInfo propriedade)
    {
        var paramRegistro = Expression.Parameter(typeof(RegistroSped), "registro");

        Expression registroTipado = Expression.Convert(paramRegistro, propriedade.DeclaringType!);
        Expression leitura = Expression.Property(registroTipado, propriedade);
        Expression boxed = Expression.Convert(leitura, typeof(object));

        return Expression.Lambda<Func<RegistroSped, object?>>(boxed, paramRegistro).Compile();
    }

    /// <summary>
    /// Compõe o delegate público <see cref="MetadadosCampo.Definidor(RegistroSped, ReadOnlySpan{char})"/>
    /// a partir da propriedade e do atributo: parse do texto (via conversor) + atribuição (via setter).
    /// <paramref name="validarDominio"/> escolhe a política de conversão de enum fechado, repassada a
    /// <see cref="ConstruirConversor"/> — é o único ponto de diferença entre o definidor permissivo
    /// (o padrão, preservado byte a byte dos três pacotes já publicados) e o estrito.
    /// </summary>
    private static Action<RegistroSped, ReadOnlySpan<char>> ComporDefinidor(
        PropertyInfo propriedade, CampoSpedAttribute atributo, bool validarDominio)
    {
        var conversor = ConstruirConversor(propriedade.PropertyType, atributo, validarDominio);
        var setter = ConstruirSetter(propriedade);
        return (registro, valor) =>
        {
            string texto = valor.IsEmpty ? string.Empty : valor.ToString();
            object? convertido = conversor(texto);
            setter(registro, convertido);
        };
    }

    /// <summary>
    /// Só enum fechado sem <c>[SpedValor]</c> e sem <c>[Flags]</c> tem política de domínio: os
    /// enums textuais já rejeitam token desconhecido no caminho permissivo, e um <c>[Flags]</c>
    /// não tem domínio fechado a validar (combinações de bits são válidas mesmo sem membro
    /// nomeado) — construir um definidor estrito para ele faria bypass do <c>Enum.Parse</c>
    /// usado pelo permissivo, perdendo o parsing por nome de membro e a forma separada por
    /// vírgula. Espelha <c>RegistroSpedCatalogoGenerator.RequiresStrictSetter</c>, para que o
    /// catálogo reflexivo e o gerado concordem.
    /// </summary>
    private static bool RequiresStrictSetter(Type tipo, CampoSpedAttribute atributo)
    {
        Type alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (!alvo.IsEnum || alvo.IsDefined(typeof(FlagsAttribute), inherit: false))
            return false;

        return !alvo.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Any(campo => campo.IsDefined(typeof(SpedValorAttribute), inherit: false));
    }

    private static Func<string, object?> ConstruirConversor(Type tipo, CampoSpedAttribute atributo, bool validarDominio)
    {
        var subjacente = Nullable.GetUnderlyingType(tipo);
        bool ehNulavel = subjacente is not null;
        var alvo = subjacente ?? tipo;

        Func<string, object?> conversorAlvo = SelecionarConversor(alvo, atributo, validarDominio);

        if (ehNulavel || !alvo.IsValueType)
        {
            return s => string.IsNullOrEmpty(s) ? null : conversorAlvo(s);
        }

        // Tipo-valor não anulável: vazio retorna default(T).
        object valorPadrao = Activator.CreateInstance(alvo)!;
        return s => string.IsNullOrEmpty(s) ? valorPadrao : conversorAlvo(s);
    }

    private static Func<string, object?> SelecionarConversor(Type alvo, CampoSpedAttribute atributo, bool validarDominio)
    {
        if (alvo == typeof(string))
            return static s => s;

        if (alvo == typeof(int))
            return static s => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        if (alvo == typeof(long))
            return static s => long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        if (alvo == typeof(short))
            return static s => short.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        if (alvo == typeof(decimal))
            return static s => ParseadoresPrimitivos.ParaDecimal(s);

        if (alvo == typeof(DateOnly))
        {
            string? formato = atributo.Formato;
            return s => ParseadoresPrimitivos.DataComFormato(s, formato);
        }

        if (alvo == typeof(bool))
            return static s => bool.Parse(s);

        if (alvo == typeof(char))
            return static s => s.Length == 1
                ? s[0]
                : throw new FormatException($"Esperado 1 caractere, recebeu '{s}'.");

        if (alvo.IsEnum)
            return ConstruirConversorEnum(alvo, atributo, validarDominio);

        if (ConversoresPrimitivosCatalogo.TentarObter(alvo, out var registrado))
            return registrado;

        throw new NotSupportedException(
            $"Tipo de campo SPED não suportado pelo CatalogoBuilder: {alvo.FullName}. " +
            "Registre um conversor via ConversoresPrimitivosCatalogo.Registrar<T>().");
    }

    private static Func<object, string> ConstruirSerializador(Type tipo, CampoSpedAttribute atributo)
    {
        var alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;
        return SelecionarSerializador(alvo, atributo);
    }

    private static Func<object, string> SelecionarSerializador(Type alvo, CampoSpedAttribute atributo)
    {
        if (alvo == typeof(string))
            return static v => (string)v;

        if (alvo == typeof(int))
            return static v => SerializadoresPrimitivos.Inteiro((int)v);

        if (alvo == typeof(long))
            return static v => SerializadoresPrimitivos.Longo((long)v);

        if (alvo == typeof(short))
            return static v => SerializadoresPrimitivos.Inteiro((short)v);

        if (alvo == typeof(decimal))
        {
            int casas = atributo.Decimais;
            return v => SerializadoresPrimitivos.DeDecimal((decimal)v, casas);
        }

        if (alvo == typeof(DateOnly))
        {
            string? formato = atributo.Formato;
            return v => SerializadoresPrimitivos.DataComFormato((DateOnly)v, formato);
        }

        if (alvo == typeof(bool))
            return static v => ((bool)v) ? "true" : "false";

        if (alvo == typeof(char))
            return static v => ((char)v).ToString();

        if (alvo.IsEnum)
            return ConstruirSerializadorEnum(alvo, atributo);

        // Value objects fiscais e demais tipos: usam ToString canônico (responsabilidade do
        // próprio tipo expor a representação SPED). Cnpj, Cfop, Ncm, etc. já fazem isso.
        return static v => v.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Conversor de enum a partir do texto SPED. Quando o enum tem membros <c>[SpedValor]</c>, o
    /// mapeamento textual é a única política — sempre estrito por natureza (token desconhecido já
    /// não tem entrada no dicionário). Quando é um enum numérico fechado sem <c>[SpedValor]</c>,
    /// <paramref name="validarDominio"/> escolhe entre o permissivo (o padrão histórico dos três
    /// pacotes já publicados: <see cref="Enum.Parse(Type, string, bool)"/> aceita código fora do
    /// domínio via cast e nome de membro) e o estrito (valida <see cref="Enum.IsDefined"/>).
    /// <paramref name="validarDominio"/> só chega <c>true</c> aqui quando
    /// <see cref="RequiresStrictSetter"/> mandou construir o definidor estrito, e esse
    /// predicado já exclui enums <see cref="FlagsAttribute"/> — não têm domínio fechado a validar
    /// e o caminho estrito, ao pular <c>Enum.Parse</c>, perderia o parsing por nome/combinação
    /// separada por vírgula.
    /// </summary>
    private static Func<string, object> ConstruirConversorEnum(
        Type alvo,
        CampoSpedAttribute atributo,
        bool validarDominio)
    {
        var porValorSped = alvo
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => new
            {
                Valor = campo.GetCustomAttribute<SpedValorAttribute>()?.Valor,
                Enum = campo.GetValue(null),
            })
            .Where(item => item.Valor is not null)
            .ToDictionary(item => item.Valor!, item => item.Enum, StringComparer.Ordinal);

        if (porValorSped.Count == 0)
        {
            if (!validarDominio)
                return s => Enum.Parse(alvo, s, ignoreCase: false);

            Func<string, object> conversorSubjacente =
                ConstruirConversorIntegral(Enum.GetUnderlyingType(alvo));
            return s =>
            {
                object valor = Enum.ToObject(alvo, conversorSubjacente(s));
                if (!Enum.IsDefined(alvo, valor))
                    throw new FormatException($"Valor '{s}' não é válido para {alvo.Name}.");
                return valor;
            };
        }

        return s => porValorSped.TryGetValue(s, out var valor)
            ? valor!
            : throw new FormatException($"Valor '{s}' não é válido para {alvo.Name}.");
    }

    private static Func<string, object> ConstruirConversorIntegral(Type tipo)
    {
        if (tipo == typeof(byte))
            return static s => byte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(sbyte))
            return static s => sbyte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(short))
            return static s => short.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(ushort))
            return static s => ushort.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(int))
            return static s => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(uint))
            return static s => uint.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(long))
            return static s => long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (tipo == typeof(ulong))
            return static s => ulong.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        throw new NotSupportedException($"Tipo integral de enum não suportado: {tipo.FullName}.");
    }

    private static Func<object, string> ConstruirSerializadorEnum(Type alvo, CampoSpedAttribute atributo)
    {
        var porMembro = alvo
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => new
            {
                ValorEnum = campo.GetValue(null)!,
                ValorSped = campo.GetCustomAttribute<SpedValorAttribute>()?.Valor,
            })
            .Where(item => item.ValorSped is not null)
            .ToDictionary(item => item.ValorEnum, item => item.ValorSped!);

        if (porMembro.Count > 0)
        {
            return v => porMembro.TryGetValue(v, out var valor)
                ? valor
                : throw new FormatException($"Valor '{v}' não tem código SPED em {alvo.Name}.");
        }

        // Em SPED o valor de um campo enumerado numérico é sempre o código do underlying,
        // com largura fixa quando o layout declara Tam=NNN*.
        int tamanho = atributo.Tamanho;
        return v =>
        {
            long underlying = ((IConvertible)v).ToInt64(CultureInfo.InvariantCulture);
            string texto = underlying.ToString(CultureInfo.InvariantCulture);
            return tamanho > 0 ? texto.PadLeft(tamanho, '0') : texto;
        };
    }

    private sealed class CatalogoReflexivo(Dictionary<string, MetadadosRegistro> registros)
        : CatalogoSpedBase(registros)
    {
    }
}
