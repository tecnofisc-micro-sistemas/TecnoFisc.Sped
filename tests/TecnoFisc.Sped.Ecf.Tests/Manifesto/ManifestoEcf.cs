using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Json.Schema;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

internal sealed partial class ManifestoEcf
{
    private const string Draft202012 = "https://json-schema.org/draft/2020-12/schema";
    private const int QuantidadeRegistrosLeiaute12 = 180;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly Dictionary<string, ManifestoRegistroEcf> _porCodigo;

    private ManifestoEcf(
        ManifestoRegistroEcf[] registros,
        string[] codigosCanonicos)
    {
        Registros = registros;
        CodigosCanonicos = codigosCanonicos;
        _porCodigo = registros.ToDictionary(registro => registro.Code, StringComparer.Ordinal);
    }

    public IReadOnlyList<ManifestoRegistroEcf> Registros { get; }

    public IReadOnlyList<string> CodigosCanonicos { get; }

    public ManifestoRegistroEcf Obter(string codigo)
        => _porCodigo.TryGetValue(codigo, out var registro)
            ? registro
            : throw new InvalidDataException(
                $"Código '{codigo}' não existe no manifesto revisado do leiaute 12.");

    public static ManifestoEcf Carregar()
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Manifesto");
        string caminhoManifesto = Path.Combine(diretorio, "layout-12-manifest.json");
        string caminhoSchema = Path.Combine(diretorio, "layout-12-manifest.schema.json");

        ExigirArquivoCopiado(caminhoManifesto, "manifesto");
        ExigirArquivoCopiado(caminhoSchema, "schema");

        return Parse(File.ReadAllText(caminhoManifesto), File.ReadAllText(caminhoSchema));
    }

    internal static ManifestoEcf Parse(string manifestoJson, string schemaJson)
    {
        ArgumentNullException.ThrowIfNull(manifestoJson);
        ArgumentNullException.ThrowIfNull(schemaJson);

        using var schema = ParseSchema(schemaJson);
        var raizSchema = schema.RootElement;
        JsonSchema schemaCompilado = BuildSchema(raizSchema);
        var canonicos = LerSequenciaCanonica(raizSchema);

        using var manifesto = ParseManifestoJson(manifestoJson);
        ValidarCodigosCanonicosJson(manifesto.RootElement, canonicos);
        ValidarContraSchema(manifesto.RootElement, schemaCompilado);

        ManifestoRegistroEcf[] registros;
        try
        {
            registros = JsonSerializer.Deserialize<ManifestoRegistroEcf[]>(manifestoJson, _jsonOptions)
                ?? throw new InvalidDataException("O manifesto do leiaute 12 não pode ser nulo.");
        }
        catch (JsonException excecao)
        {
            throw new InvalidDataException(
                $"O manifesto do leiaute 12 não respeita a estrutura JSON esperada: {excecao.Message}",
                excecao);
        }

        ValidarRegistros(registros, canonicos);
        return new ManifestoEcf(
            registros,
            canonicos.Select(item => item.Code).ToArray());
    }

    private static void ValidarCodigosCanonicosJson(JsonElement manifesto, Canonico[] canonicos)
    {
        if (manifesto.ValueKind is not JsonValueKind.Array)
            return;

        var codigos = new List<string>(manifesto.GetArrayLength());
        foreach (var registro in manifesto.EnumerateArray())
        {
            if (registro.ValueKind is not JsonValueKind.Object ||
                !registro.TryGetProperty("code", out var codigo) ||
                codigo.ValueKind is not JsonValueKind.String)
            {
                return;
            }

            codigos.Add(codigo.GetString()!);
        }

        var primeiraPosicao = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int indice = 0; indice < codigos.Count; indice++)
        {
            string codigo = codigos[indice];
            if (!primeiraPosicao.TryAdd(codigo, indice))
            {
                throw new InvalidDataException(
                    $"Código duplicado '{codigo}' no manifesto ECF, posições " +
                    $"{primeiraPosicao[codigo] + 1} e {indice + 1}.");
            }
        }

        var codigosSchema = canonicos.Select(item => item.Code).ToHashSet(StringComparer.Ordinal);
        for (int indice = 0; indice < codigos.Count; indice++)
        {
            string codigo = codigos[indice];
            if (!codigosSchema.Contains(codigo))
            {
                throw new InvalidDataException(
                    $"O código '{codigo}' na posição {indice + 1} não existe no schema canônico do leiaute 12.");
            }

            if (indice < canonicos.Length &&
                !string.Equals(codigo, canonicos[indice].Code, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Divergência na ordem canônica na posição {indice + 1}: " +
                    $"esperado '{canonicos[indice].Code}', encontrado '{codigo}'.");
            }
        }
    }

    private static JsonDocument ParseSchema(string schemaJson)
    {
        try
        {
            var schema = JsonDocument.Parse(schemaJson);
            ValidarCabecalhoSchema(schema.RootElement);
            return schema;
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception excecao) when (
            excecao is JsonException or JsonSchemaException or InvalidOperationException or KeyNotFoundException)
        {
            throw new InvalidDataException(
                $"O schema ECF Draft 2020-12 está malformado: {excecao.Message}",
                excecao);
        }
    }

    private static JsonSchema BuildSchema(JsonElement schemaJson)
    {
        try
        {
            return JsonSchema.Build(
                schemaJson,
                new BuildOptions
                {
                    Dialect = Dialect.Draft202012,
                    SchemaRegistry = new SchemaRegistry(),
                });
        }
        catch (Exception excecao) when (excecao is JsonSchemaException or InvalidOperationException)
        {
            throw new InvalidDataException(
                $"O schema ECF Draft 2020-12 está malformado em prefixItems/keywords: {excecao.Message}",
                excecao);
        }
    }

    private static JsonDocument ParseManifestoJson(string manifestoJson)
    {
        try
        {
            return JsonDocument.Parse(manifestoJson);
        }
        catch (JsonException excecao)
        {
            throw new InvalidDataException(
                $"O manifesto ECF não contém JSON válido: {excecao.Message}",
                excecao);
        }
    }

    private static void ValidarContraSchema(JsonElement manifesto, JsonSchema schema)
    {
        var resultado = schema.Evaluate(
            manifesto,
            new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (resultado.IsValid)
            return;

        var erros = (resultado.Details ?? [])
            .Where(item => !item.IsValid && item.Errors is not null)
            .Select(item => FormatarErroSchema(item, manifesto))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        throw new InvalidDataException(
            "O manifesto ECF viola o schema Draft 2020-12:" + Environment.NewLine +
            string.Join(Environment.NewLine, erros.Select(erro => $"- {erro}")));
    }

    private static string FormatarErroSchema(EvaluationResults resultado, JsonElement manifesto)
    {
        string caminho = resultado.InstanceLocation.ToString();
        string contexto = ContextoRegistro(caminho, manifesto);
        string detalhes = string.Join(
            "; ",
            resultado.Errors!.Select(erro => $"{erro.Key}: {erro.Value}"));
        return $"{contexto}{caminho}: {detalhes}";
    }

    private static string ContextoRegistro(string caminho, JsonElement manifesto)
    {
        if (manifesto.ValueKind is not JsonValueKind.Array ||
            caminho.Length < 2 ||
            caminho[0] != '/')
        {
            return string.Empty;
        }

        int fimIndice = caminho.IndexOf('/', 1);
        ReadOnlySpan<char> textoIndice = fimIndice < 0
            ? caminho.AsSpan(1)
            : caminho.AsSpan(1, fimIndice - 1);
        if (!int.TryParse(textoIndice, NumberStyles.None, CultureInfo.InvariantCulture, out int indice) ||
            indice < 0 ||
            indice >= manifesto.GetArrayLength())
        {
            return string.Empty;
        }

        var registro = manifesto[indice];
        if (!registro.TryGetProperty("code", out var codigo) || codigo.ValueKind is not JsonValueKind.String)
            return string.Empty;

        return $"registro {codigo.GetString()} em ";
    }

    private static void ExigirArquivoCopiado(string caminho, string tipo)
    {
        if (!File.Exists(caminho))
        {
            throw new InvalidDataException(
                $"O {tipo} ECF não foi copiado para o output de testes: '{caminho}'. " +
                "Verifique os itens Link/CopyToOutputDirectory do projeto de testes.");
        }
    }

    private static void ValidarCabecalhoSchema(JsonElement schema)
    {
        string? draft = schema.GetProperty("$schema").GetString();
        if (!string.Equals(draft, Draft202012, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"O schema ECF deve usar JSON Schema Draft 2020-12 ('{Draft202012}'); encontrado '{draft}'.");
        }

        string? id = schema.GetProperty("$id").GetString();
        if (id is null || !id.EndsWith("/sped/ecf/layout-12-manifest.schema.json", StringComparison.Ordinal))
            throw new InvalidDataException($"O $id do schema não identifica o manifesto ECF leiaute 12: '{id}'.");

        if (schema.GetProperty("type").GetString() != "array" ||
            schema.GetProperty("minItems").GetInt32() != QuantidadeRegistrosLeiaute12 ||
            schema.GetProperty("maxItems").GetInt32() != QuantidadeRegistrosLeiaute12 ||
            !schema.GetProperty("uniqueItems").GetBoolean() ||
            schema.GetProperty("items").ValueKind is not JsonValueKind.False)
        {
            throw new InvalidDataException(
                "O schema ECF deve fechar o array em exatamente 180 itens únicos e proibir itens fora de prefixItems.");
        }
    }

    private static Canonico[] LerSequenciaCanonica(JsonElement schema)
    {
        var itens = schema.GetProperty("prefixItems");
        if (itens.GetArrayLength() != QuantidadeRegistrosLeiaute12)
        {
            throw new InvalidDataException(
                $"O schema ECF deve declarar 180 prefixItems; encontrou {itens.GetArrayLength()}.");
        }

        var resultado = new Canonico[QuantidadeRegistrosLeiaute12];
        int indice = 0;
        foreach (var item in itens.EnumerateArray())
        {
            JsonElement? propriedades = null;
            foreach (var parte in item.GetProperty("allOf").EnumerateArray())
            {
                if (parte.TryGetProperty("properties", out var candidatas))
                    propriedades = candidatas;
            }

            if (propriedades is null ||
                !propriedades.Value.TryGetProperty("code", out var codigoSchema) ||
                !codigoSchema.TryGetProperty("const", out var codigoConst) ||
                !propriedades.Value.TryGetProperty("block", out var blocoSchema) ||
                !blocoSchema.TryGetProperty("const", out var blocoConst))
            {
                throw new InvalidDataException(
                    $"prefixItems[{indice}] não fixa code e block para o leiaute 12.");
            }

            resultado[indice++] = new Canonico(
                codigoConst.GetString()!,
                blocoConst.GetString()!);
        }

        return resultado;
    }

    private static void ValidarRegistros(
        ManifestoRegistroEcf[] registros,
        Canonico[] canonicos)
    {
        if (registros.Length != QuantidadeRegistrosLeiaute12)
        {
            throw new InvalidDataException(
                $"O manifesto ECF deve conter exatamente 180 registros; encontrou {registros.Length}.");
        }

        var primeiraPosicao = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int indice = 0; indice < registros.Length; indice++)
        {
            string codigo = registros[indice].Code;
            if (!primeiraPosicao.TryAdd(codigo, indice))
            {
                throw new InvalidDataException(
                    $"Código duplicado '{codigo}' no manifesto ECF, posições " +
                    $"{primeiraPosicao[codigo] + 1} e {indice + 1}.");
            }
        }

        var codigosSchema = canonicos.Select(item => item.Code).ToHashSet(StringComparer.Ordinal);
        for (int indice = 0; indice < registros.Length; indice++)
        {
            var registro = registros[indice];
            if (!codigosSchema.Contains(registro.Code))
            {
                throw new InvalidDataException(
                    $"O código '{registro.Code}' na posição {indice + 1} não existe no schema canônico do leiaute 12.");
            }

            var esperado = canonicos[indice];
            if (!string.Equals(registro.Code, esperado.Code, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Divergência na ordem canônica na posição {indice + 1}: " +
                    $"esperado '{esperado.Code}', encontrado '{registro.Code}'.");
            }

            ValidarRegistro(registro, esperado);
        }
    }

    private static void ValidarRegistro(ManifestoRegistroEcf registro, Canonico esperado)
    {
        if (!string.Equals(registro.Block, esperado.Block, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Registro {registro.Code} declara bloco '{registro.Block}', mas o schema fixa '{esperado.Block}'.");
        }

        if (!int.TryParse(registro.Level, out _))
            throw new InvalidDataException($"Registro {registro.Code} tem nível inválido '{registro.Level}'.");
        if (!OccurrencePattern().IsMatch(registro.Occurrence))
            throw new InvalidDataException($"Registro {registro.Code} tem ocorrência inválida '{registro.Occurrence}'.");
        if (!registro.Reviewed)
            throw new InvalidDataException($"Registro {registro.Code} ainda não foi revisado.");
        if (registro.PageStart < 1 || registro.PageEnd < registro.PageStart)
            throw new InvalidDataException($"Registro {registro.Code} tem intervalo de páginas inválido.");
        if (registro.Fields.Count == 0)
            throw new InvalidDataException($"Registro {registro.Code} não declara campos.");

        for (int campoIndice = 0; campoIndice < registro.Fields.Count; campoIndice++)
        {
            var campo = registro.Fields[campoIndice];
            int numeroEsperado = campoIndice + 1;
            if (campo.Number != numeroEsperado)
            {
                throw new InvalidDataException(
                    $"Registro {registro.Code}: sequência de campos inválida; esperado nº {numeroEsperado}, " +
                    $"encontrado nº {campo.Number} ({campo.Name}).");
            }

            if (campoIndice == 0 && !string.Equals(campo.Name, "REG", StringComparison.Ordinal))
                throw new InvalidDataException($"Registro {registro.Code}: o campo nº 1 deve ser REG.");

            if (campo.Required is not ("Sim" or "Não" or "S" or "N" or "-" or "Sim”" or "sim"))
            {
                throw new InvalidDataException(
                    $"O schema semântico do registro {registro.Code}, fields/{campoIndice}/required " +
                    $"não reconhece o marcador '{campo.Required}'.");
            }
        }
    }

    [GeneratedRegex("^[01]:(?:[1-9][0-9]*|N)$", RegexOptions.CultureInvariant)]
    private static partial Regex OccurrencePattern();

    private sealed record Canonico(string Code, string Block);
}

internal sealed record ManifestoRegistroEcf
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("block")]
    public required string Block { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("pageStart")]
    public required int PageStart { get; init; }

    [JsonPropertyName("pageEnd")]
    public required int PageEnd { get; init; }

    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("occurrence")]
    public required string Occurrence { get; init; }

    [JsonPropertyName("reviewed")]
    public required bool Reviewed { get; init; }

    [JsonPropertyName("fields")]
    public required IReadOnlyList<ManifestoCampoEcf> Fields { get; init; }
}

internal sealed record ManifestoCampoEcf
{
    [JsonPropertyName("number")]
    public required int Number { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("size")]
    public required string Size { get; init; }

    [JsonPropertyName("decimals")]
    public required string Decimals { get; init; }

    [JsonPropertyName("required")]
    public required string Required { get; init; }

    [JsonPropertyName("validValues")]
    public required string ValidValues { get; init; }
}
