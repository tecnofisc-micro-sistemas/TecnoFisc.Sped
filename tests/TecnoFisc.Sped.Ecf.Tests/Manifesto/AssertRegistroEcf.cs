using System.Globalization;
using System.Text;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

internal static class AssertRegistroEcf
{
    private static readonly CatalogoSpedGerado _catalogo = new();
    private static readonly Lazy<ManifestoEcf> _manifesto = new(ManifestoEcf.Carregar);

    public static void CodesAreImplemented(params string[] codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var manifesto = _manifesto.Value;
        var codigosManifesto = manifesto.CodigosCanonicos.ToHashSet(StringComparer.Ordinal);
        var codigosCatalogo = _catalogo.EnumerarRegistros()
            .Select(registro => registro.Codigo)
            .ToHashSet(StringComparer.Ordinal);

        var solicitadosDuplicados = codes
            .GroupBy(codigo => codigo, StringComparer.Ordinal)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToArray();
        var foraDoManifesto = codes
            .Where(codigo => !codigosManifesto.Contains(codigo))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var ausentes = codes
            .Where(codigo => codigosManifesto.Contains(codigo) && !codigosCatalogo.Contains(codigo))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var extrasCatalogo = codigosCatalogo
            .Where(codigo => !codigosManifesto.Contains(codigo))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var divergencias = new List<string>();
        AdicionarCodigos(divergencias, "códigos solicitados duplicados", solicitadosDuplicados);
        AdicionarCodigos(divergencias, "códigos solicitados fora do manifesto", foraDoManifesto);
        AdicionarCodigos(divergencias, "códigos ausentes do catálogo", ausentes);
        AdicionarCodigos(divergencias, "códigos extras no catálogo", extrasCatalogo);

        FalharSeHouverDivergencias(divergencias);
    }

    public static void CatalogMatchesManifest()
    {
        var manifesto = _manifesto.Value;
        var codigosManifesto = manifesto.CodigosCanonicos.ToHashSet(StringComparer.Ordinal);
        var metadadosCatalogo = _catalogo.EnumerarRegistros().ToArray();
        var divergencias = new List<string>();

        var codigosDesconhecidos = metadadosCatalogo
            .Select(registro => registro.Codigo)
            .Where(codigo => !codigosManifesto.Contains(codigo))
            .Order(StringComparer.Ordinal)
            .ToArray();
        AdicionarCodigos(divergencias, "códigos extras no catálogo", codigosDesconhecidos);

        foreach (var metadados in metadadosCatalogo)
        {
            if (!codigosManifesto.Contains(metadados.Codigo))
                continue;

            CompararMetadados(manifesto.Obter(metadados.Codigo), metadados, divergencias);
        }

        FalharSeHouverDivergencias(divergencias);
    }

    internal static void MetadataMatchesManifest(MetadadosRegistro metadados)
    {
        ArgumentNullException.ThrowIfNull(metadados);

        var divergencias = new List<string>();
        CompararMetadados(_manifesto.Value.Obter(metadados.Codigo), metadados, divergencias);
        FalharSeHouverDivergencias(divergencias);
    }

    public static void ConformsToManifest(
        RegistroSped registro,
        string codigo,
        string ocorrencia,
        RegistroSped? pai = null,
        params RegistroSped[] filhos)
    {
        ArgumentNullException.ThrowIfNull(registro);
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(ocorrencia);
        ArgumentNullException.ThrowIfNull(filhos);

        CodesAreImplemented(codigo);
        var manifesto = _manifesto.Value;
        var esperado = manifesto.Obter(codigo);
        var divergencias = new List<string>();

        if (!string.Equals(registro.Codigo, codigo, StringComparison.Ordinal))
        {
            divergencias.Add(
                $"registro materializado: código esperado '{codigo}', encontrado '{registro.Codigo}'");
        }

        if (!_catalogo.TentarObter(codigo, out var metadados))
            divergencias.Add($"registro {codigo}: metadados ausentes do catálogo gerado");
        else
            CompararMetadados(esperado, metadados, divergencias);

        if (!string.Equals(esperado.Occurrence, ocorrencia, StringComparison.Ordinal))
        {
            divergencias.Add(
                $"registro {codigo}: ocorrência esperada '{esperado.Occurrence}', informada '{ocorrencia}'");
        }

        if (!ReferenceEquals(registro.Pai, pai))
        {
            divergencias.Add(
                $"registro {codigo}: Pai esperado '{pai?.Codigo ?? "<raiz>"}', " +
                $"encontrado '{registro.Pai?.Codigo ?? "<raiz>"}'");
        }

        if (registro.Filhos.Count != filhos.Length ||
            !registro.Filhos.Zip(filhos).All(par => ReferenceEquals(par.First, par.Second)))
        {
            divergencias.Add(
                $"registro {codigo}: Filhos esperados [{FormatarCodigos(filhos.Select(item => item.Codigo))}], " +
                $"encontrados [{FormatarCodigos(registro.Filhos.Select(item => item.Codigo))}]");
        }

        FalharSeHouverDivergencias(divergencias);
    }

    private static void CompararMetadados(
        ManifestoRegistroEcf esperado,
        MetadadosRegistro atual,
        List<string> divergencias)
    {
        AdicionarDivergencia(
            divergencias,
            esperado.Code,
            "código",
            esperado.Code,
            atual.Codigo);
        AdicionarDivergencia(
            divergencias,
            esperado.Code,
            "bloco",
            esperado.Block,
            atual.Bloco);
        AdicionarDivergencia(
            divergencias,
            esperado.Code,
            "nível",
            int.Parse(esperado.Level, CultureInfo.InvariantCulture),
            atual.Nivel);

        string codigoDaInstancia = atual.Fabrica().Codigo;
        AdicionarDivergencia(
            divergencias,
            esperado.Code,
            "código da instância criada pelo catálogo",
            esperado.Code,
            codigoDaInstancia);

        var camposEsperados = esperado.Fields.Skip(1).ToArray();
        if (camposEsperados.Length != atual.Campos.Count)
        {
            divergencias.Add(
                $"registro {esperado.Code}: quantidade de campos esperada {camposEsperados.Length} " +
                $"(REG excluído), encontrada {atual.Campos.Count}");
        }

        int quantidadeComparavel = Math.Min(camposEsperados.Length, atual.Campos.Count);
        for (int indice = 0; indice < quantidadeComparavel; indice++)
            CompararCampo(esperado.Code, camposEsperados[indice], atual.Campos[indice], divergencias);
    }

    private static void CompararCampo(
        string codigo,
        ManifestoCampoEcf esperado,
        MetadadosCampo atual,
        List<string> divergencias)
    {
        string contexto = $"registro {codigo}, campo nº {esperado.Number} {esperado.Name}";
        AdicionarDivergencia(divergencias, contexto, "ordem", esperado.Number, atual.Ordem);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "nome",
            NormalizarNome(esperado.Name),
            NormalizarNome(atual.Nome));
        AdicionarDivergencia(
            divergencias,
            contexto,
            "tamanho",
            NormalizarTamanho(esperado.Size),
            atual.Tamanho);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "decimais",
            NormalizarDecimais(esperado.Decimals),
            atual.Decimais);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "obrigatório",
            NormalizarObrigatorio(esperado.Required),
            atual.Obrigatorio);

        if (!TipoCompativel(esperado, atual.Tipo))
        {
            divergencias.Add(
                $"{contexto}: tipo do manifesto '{esperado.Type}' incompatível com CLR " +
                $"'{atual.Tipo.FullName}'");
        }
    }

    private static bool TipoCompativel(ManifestoCampoEcf campo, Type tipo)
    {
        Type alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;

        if (campo.Type == "D")
            return alvo == typeof(DateOnly);

        if (campo.Type == "NS")
            return alvo == typeof(decimal);

        if (campo.Type == "C")
        {
            if (NomeRepresentaCnpj(campo))
                return alvo.Name == "Cnpj";
            if (NomeRepresentaCpf(campo))
                return alvo.Name == "Cpf";

            return alvo == typeof(string) || alvo == typeof(char) || alvo.IsEnum;
        }

        if (campo.Type != "N")
            return false;

        if (NomeRepresentaData(campo))
            return alvo == typeof(DateOnly);

        if (NormalizarDecimais(campo.Decimals) > 0)
            return alvo == typeof(decimal);

        return alvo == typeof(short) ||
               alvo == typeof(int) ||
               alvo == typeof(long) ||
               alvo == typeof(decimal) ||
               alvo == typeof(string) ||
               alvo.IsEnum;
    }

    private static bool NomeRepresentaCnpj(ManifestoCampoEcf campo)
        => campo.Name.Contains("CNPJ", StringComparison.Ordinal) ||
           campo.Name is "COD_SCP";

    private static bool NomeRepresentaCpf(ManifestoCampoEcf campo)
        => campo.Name.Contains("CPF", StringComparison.Ordinal);

    private static bool NomeRepresentaData(ManifestoCampoEcf campo)
        => (campo.Name.StartsWith("DT_", StringComparison.Ordinal) ||
            campo.Name.StartsWith("VIG_", StringComparison.Ordinal)) &&
           NormalizarTamanho(campo.Size) == 8;

    private static int NormalizarTamanho(string valor)
    {
        if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out int tamanho))
            return tamanho;

        int abreParenteses = valor.IndexOf('(');
        int fechaParenteses = valor.IndexOf(')', abreParenteses + 1);
        if (abreParenteses >= 0 && fechaParenteses > abreParenteses + 1 &&
            int.TryParse(
                valor.AsSpan(abreParenteses + 1, fechaParenteses - abreParenteses - 1),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out tamanho))
        {
            return tamanho;
        }

        return 0;
    }

    private static int NormalizarDecimais(string valor)
        => int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out int decimais)
            ? decimais
            : 0;

    private static bool NormalizarObrigatorio(string valor)
    {
        string normalizado = NormalizarNome(valor);
        return normalizado is "SIM" or "S";
    }

    private static string NormalizarNome(string valor)
    {
        var resultado = new StringBuilder(valor.Length);
        foreach (char caractere in valor.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark ||
                !char.IsLetterOrDigit(caractere))
            {
                continue;
            }

            resultado.Append(char.ToUpperInvariant(caractere));
        }

        return resultado.ToString();
    }

    private static void AdicionarDivergencia<T>(
        List<string> divergencias,
        string contexto,
        string propriedade,
        T esperado,
        T atual)
    {
        if (!EqualityComparer<T>.Default.Equals(esperado, atual))
            divergencias.Add($"{contexto}: {propriedade} esperado '{esperado}', encontrado '{atual}'");
    }

    private static void AdicionarCodigos(
        List<string> divergencias,
        string rotulo,
        string[] codigos)
    {
        if (codigos.Length > 0)
            divergencias.Add($"{rotulo}: [{FormatarCodigos(codigos)}]");
    }

    private static string FormatarCodigos(IEnumerable<string> codigos)
        => string.Join(", ", codigos.Select(codigo => $"'{codigo}'"));

    private static void FalharSeHouverDivergencias(List<string> divergencias)
    {
        if (divergencias.Count == 0)
            return;

        throw new Xunit.Sdk.XunitException(
            "Divergências entre manifesto e implementação ECF:" + Environment.NewLine +
            string.Join(Environment.NewLine, divergencias.Select(item => $"- {item}")));
    }
}
