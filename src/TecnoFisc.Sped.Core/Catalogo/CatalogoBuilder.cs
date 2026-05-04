using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.Core.Catalogo;

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
        return new MetadadosRegistro(
            atributo.Codigo,
            atributo.Nivel,
            atributo.Bloco,
            tipo,
            fabrica,
            campos);
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

        foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var atributo = propriedade.GetCustomAttribute<CampoSpedAttribute>(inherit: true);
            if (atributo is null)
                continue;

            if (!ordensVistas.Add(atributo.Ordem))
                throw new InvalidOperationException(
                    $"Ordem duplicada {atributo.Ordem} em {tipo.FullName}.{propriedade.Name}.");

            if (propriedade.SetMethod is null)
                throw new InvalidOperationException(
                    $"Propriedade {tipo.FullName}.{propriedade.Name} precisa de setter para receber valor SPED.");

            var conversor = ConstruirConversor(propriedade.PropertyType, atributo);
            var setter = ConstruirSetter(propriedade);

            lista.Add((atributo.Ordem, new MetadadosCampo(
                propriedade.Name,
                atributo.Ordem,
                propriedade.PropertyType,
                atributo.Tamanho,
                atributo.Decimais,
                atributo.Obrigatorio,
                atributo.Formato,
                conversor,
                setter)));
        }

        lista.Sort(static (a, b) => a.Ordem.CompareTo(b.Ordem));

        // Detecta lacunas — ordens devem ser sequenciais a partir de 1.
        for (int i = 0; i < lista.Count; i++)
        {
            int esperado = i + 1;
            if (lista[i].Ordem != esperado)
                throw new InvalidOperationException(
                    $"Ordens de campos de {tipo.FullName} devem ser sequenciais a partir de 1; " +
                    $"esperava {esperado}, encontrei {lista[i].Ordem} ({lista[i].Campo.Nome}).");
        }

        return lista.Count == 0
            ? Array.Empty<MetadadosCampo>()
            : lista.ConvertAll(static x => x.Campo);
    }

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

    private static Func<string, object?> ConstruirConversor(Type tipo, CampoSpedAttribute atributo)
    {
        var subjacente = Nullable.GetUnderlyingType(tipo);
        bool ehNulavel = subjacente is not null;
        var alvo = subjacente ?? tipo;

        Func<string, object?> conversorAlvo = SelecionarConversor(alvo, atributo);

        if (ehNulavel || !alvo.IsValueType)
        {
            return s => string.IsNullOrEmpty(s) ? null : conversorAlvo(s);
        }

        // Tipo-valor não anulável: vazio retorna default(T).
        object valorPadrao = Activator.CreateInstance(alvo)!;
        return s => string.IsNullOrEmpty(s) ? valorPadrao : conversorAlvo(s);
    }

    private static Func<string, object?> SelecionarConversor(Type alvo, CampoSpedAttribute atributo)
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
            return s => Enum.Parse(alvo, s, ignoreCase: false);

        if (ConversoresPrimitivosCatalogo.TentarObter(alvo, out var registrado))
            return registrado;

        throw new NotSupportedException(
            $"Tipo de campo SPED não suportado pelo CatalogoBuilder: {alvo.FullName}. " +
            "Registre um conversor via ConversoresPrimitivosCatalogo.Registrar<T>().");
    }

    private sealed class CatalogoReflexivo(Dictionary<string, MetadadosRegistro> registros)
        : CatalogoSpedBase(registros)
    {
    }
}
