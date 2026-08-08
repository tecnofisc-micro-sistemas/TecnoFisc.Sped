using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

internal static partial class AssertRegistroEcf
{
    private static readonly CatalogoSpedGerado _catalogo = new();
    private static readonly Lazy<ManifestoEcf> _manifesto = new(ManifestoEcf.Carregar);

    public static void CodesAreImplemented(params string[] codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var manifesto = _manifesto.Value;
        var codigosManifesto = manifesto.CodigosCanonicos.ToHashSet(StringComparer.Ordinal);
        // Registros descontinuados (DescontinuadoEm != 0) existem no catálogo para leitura de
        // arquivos históricos, mas o manifesto descreve só o leiaute 12 vigente — não fazem parte
        // desta comparação catálogo × manifesto, nem como "extras" nem como candidatos a
        // "ausentes". Ver Catalogo_SoDivergeDoManifestoNosSeteRemovidosNoLeiaute11, que trava que
        // a divergência fica restrita a esse conjunto.
        var codigosCatalogo = _catalogo.EnumerarRegistros()
            .Where(registro => registro.DescontinuadoEm == 0)
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
        => CatalogMatchesManifest(_catalogo.EnumerarRegistros());

    internal static void CatalogMatchesManifest(IEnumerable<MetadadosRegistro> catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);

        var manifesto = _manifesto.Value;
        // Mesma exclusão de CodesAreImplemented: descontinuados não fazem parte da comparação
        // catálogo × manifesto (o manifesto só descreve o leiaute 12 vigente).
        MetadadosRegistro[] metadadosCatalogo = catalogo
            .Where(registro => registro.DescontinuadoEm == 0)
            .ToArray();
        string[] codigosManifesto = manifesto.Registros.Select(registro => registro.Code).ToArray();
        string[] codigosCatalogo = metadadosCatalogo.Select(registro => registro.Codigo).ToArray();
        var conjuntoManifesto = codigosManifesto.ToHashSet(StringComparer.Ordinal);
        var conjuntoCatalogo = codigosCatalogo.ToHashSet(StringComparer.Ordinal);
        var divergencias = new List<string>();

        if (codigosCatalogo.Length != codigosManifesto.Length)
        {
            divergencias.Add(
                $"quantidade de registros esperada {codigosManifesto.Length}, " +
                $"encontrada {codigosCatalogo.Length}");
        }

        foreach (var grupo in codigosCatalogo
                     .Select((codigo, indice) => (Codigo: codigo, Posicao: indice + 1))
                     .GroupBy(item => item.Codigo, StringComparer.Ordinal)
                     .Where(grupo => grupo.Count() > 1))
        {
            divergencias.Add(
                $"código duplicado '{grupo.Key}' nas posições " +
                string.Join(", ", grupo.Select(item => item.Posicao)));
        }

        AdicionarCodigos(
            divergencias,
            "códigos ausentes do catálogo",
            codigosManifesto.Where(codigo => !conjuntoCatalogo.Contains(codigo)).ToArray());
        AdicionarCodigos(
            divergencias,
            "códigos extras no catálogo",
            codigosCatalogo.Where(codigo => !conjuntoManifesto.Contains(codigo))
                .Distinct(StringComparer.Ordinal)
                .ToArray());

        int quantidadeComparavel = Math.Min(codigosManifesto.Length, codigosCatalogo.Length);
        for (int indice = 0; indice < quantidadeComparavel; indice++)
        {
            if (string.Equals(codigosManifesto[indice], codigosCatalogo[indice], StringComparison.Ordinal))
                continue;

            divergencias.Add(
                $"ordem canônica divergente na posição {indice + 1}: " +
                $"esperado '{codigosManifesto[indice]}', encontrado '{codigosCatalogo[indice]}'");
            break;
        }

        string[] blocosManifesto = ExtrairBlocosEmOrdem(
            manifesto.Registros.Select(registro => registro.Block));
        string[] blocosCatalogo = ExtrairBlocosEmOrdem(
            metadadosCatalogo.Select(registro => registro.Bloco));
        if (!blocosManifesto.SequenceEqual(blocosCatalogo, StringComparer.Ordinal))
        {
            divergencias.Add(
                $"ordem/contiguidade dos blocos esperada [{FormatarCodigos(blocosManifesto)}], " +
                $"encontrada [{FormatarCodigos(blocosCatalogo)}]");
        }

        foreach (var metadados in metadadosCatalogo
                     .GroupBy(registro => registro.Codigo, StringComparer.Ordinal)
                     .Select(grupo => grupo.First()))
        {
            if (!conjuntoManifesto.Contains(metadados.Codigo))
                continue;

            CompararMetadados(manifesto.Obter(metadados.Codigo), metadados, divergencias);
        }

        FalharSeHouverDivergencias(divergencias);
    }

    private static string[] ExtrairBlocosEmOrdem(IEnumerable<string> blocos)
    {
        var resultado = new List<string>();
        foreach (string bloco in blocos)
        {
            if (resultado.Count == 0 || !string.Equals(resultado[^1], bloco, StringComparison.Ordinal))
                resultado.Add(bloco);
        }

        return resultado.ToArray();
    }

    internal static void MetadataMatchesManifest(MetadadosRegistro metadados)
    {
        ArgumentNullException.ThrowIfNull(metadados);

        var divergencias = new List<string>();
        CompararMetadados(_manifesto.Value.Obter(metadados.Codigo), metadados, divergencias);
        FalharSeHouverDivergencias(divergencias);
    }

    internal static void FieldMetadataMatchesManifest(
        string codigo,
        string nomeCampo,
        Type tipo,
        int tamanho,
        int decimais,
        bool obrigatorio)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(nomeCampo);
        ArgumentNullException.ThrowIfNull(tipo);

        var registro = _manifesto.Value.Obter(codigo);
        var campo = registro.Fields.SingleOrDefault(
            item => string.Equals(item.Name, nomeCampo, StringComparison.Ordinal));
        if (campo is null)
        {
            throw new Xunit.Sdk.XunitException(
                $"Registro {codigo}: campo '{nomeCampo}' não existe no manifesto.");
        }

        string contexto = $"registro {codigo}, campo nº {campo.Number} {campo.Name}";
        var divergencias = new List<string>();
        AdicionarDivergencia(
            divergencias,
            contexto,
            "tamanho",
            NormalizarTamanho(campo),
            tamanho);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "decimais",
            NormalizarDecimais(campo.Decimals),
            decimais);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "obrigatório",
            NormalizarObrigatorio(campo.Required),
            obrigatorio);
        if (!TipoCompativel(registro, campo, tipo))
        {
            divergencias.Add(
                $"{contexto}: tipo do manifesto '{campo.Type}' incompatível com CLR '{tipo.FullName}'");
        }

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
        AdicionarDivergencia(
            divergencias,
            esperado.Code,
            "versão de introdução",
            esperado.IntroducedIn,
            atual.IntroduzidoEm);

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
            CompararCampo(esperado, camposEsperados[indice], atual.Campos[indice], divergencias);
    }

    private static void CompararCampo(
        ManifestoRegistroEcf registro,
        ManifestoCampoEcf esperado,
        MetadadosCampo atual,
        List<string> divergencias)
    {
        string codigo = registro.Code;
        string contexto = $"registro {codigo}, campo nº {esperado.Number} {esperado.Name}";
        AdicionarDivergencia(divergencias, contexto, "ordem", esperado.Number, atual.Ordem);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "nome",
            esperado.Name,
            atual.Nome);
        AdicionarDivergencia(
            divergencias,
            contexto,
            "tamanho",
            NormalizarTamanho(esperado),
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
        AdicionarDivergencia(
            divergencias,
            contexto,
            "versão de introdução",
            esperado.SinceVersion,
            atual.DesdeVersao);

        if (!TipoCompativel(registro, esperado, atual.Tipo))
        {
            divergencias.Add(
                $"{contexto}: tipo do manifesto '{esperado.Type}' incompatível com CLR " +
                $"'{atual.Tipo.FullName}'");
        }
    }

    internal static bool TipoCompativel(
        ManifestoRegistroEcf registro,
        ManifestoCampoEcf campo,
        Type tipo)
    {
        string nomeCampo = CanonicalFieldName(campo.Name);
        bool campoNif = string.Equals(nomeCampo, "NIF", StringComparison.Ordinal);
        bool campoCnpj = string.Equals(nomeCampo, "CNPJ", StringComparison.Ordinal);
        bool possuiParDocumento = registro.Fields.Any(item =>
        {
            string nomeIrmao = CanonicalFieldName(item.Name);
            return campoNif
                ? string.Equals(nomeIrmao, "CNPJ", StringComparison.Ordinal)
                : campoCnpj && string.Equals(nomeIrmao, "NIF", StringComparison.Ordinal);
        });
        if (possuiParDocumento)
        {
            Type alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;
            return alvo == typeof(string);
        }

        return TipoCompativel(campo, tipo);
    }

    internal static bool TipoCompativel(ManifestoCampoEcf campo, Type tipo)
    {
        Type alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;
        CampoSemantica semantica = ClassificarSemantica(campo);

        if (semantica is CampoSemantica.Data)
            return alvo == typeof(DateOnly);

        if (semantica is CampoSemantica.Cpf)
            return alvo == typeof(Cpf);

        if (semantica is CampoSemantica.Cnpj)
            return alvo == typeof(Cnpj);

        if (semantica is CampoSemantica.DocumentoComposto)
            return alvo == typeof(string);

        if (campo.Type == "C")
            return alvo == typeof(string) ||
                   alvo == typeof(char) ||
                   alvo == typeof(Cpf) ||
                   alvo == typeof(Cnpj) ||
                   alvo.IsEnum;

        if (campo.Type == "NS")
            return alvo == typeof(decimal);

        if (campo.Type == "D")
            return alvo == typeof(DateOnly);

        if (campo.Type != "N")
            return false;

        if (NormalizarDecimais(campo.Decimals) > 0)
            return alvo == typeof(decimal);

        return alvo == typeof(short) ||
               alvo == typeof(int) ||
               alvo == typeof(long) ||
               alvo == typeof(decimal) ||
               alvo == typeof(string) ||
               alvo.IsEnum;
    }

    private static CampoSemantica ClassificarSemantica(ManifestoCampoEcf campo)
    {
        bool declaraCpf = CpfTokenPattern().IsMatch(campo.Name);
        bool declaraCnpj = CnpjTokenPattern().IsMatch(campo.Name);
        bool declaraNif = NifTokenPattern().IsMatch(campo.Name);

        int documentosDeclarados = (declaraCpf ? 1 : 0) +
                                   (declaraCnpj ? 1 : 0) +
                                   (declaraNif ? 1 : 0);
        if (documentosDeclarados > 1)
            return CampoSemantica.DocumentoComposto;

        int tamanho = NormalizarTamanho(campo);
        if (declaraCpf && tamanho == 11)
            return CampoSemantica.Cpf;
        if (declaraCnpj && tamanho == 14)
            return CampoSemantica.Cnpj;

        bool declaraData = tamanho == 8 && PossuiSegmentoData(campo.Name);
        return declaraData ? CampoSemantica.Data : CampoSemantica.Generico;
    }

    private static bool PossuiSegmentoData(string nomeCampo)
    {
        foreach (string segmento in nomeCampo.Split('_'))
        {
            if (string.Equals(segmento, "DATA", StringComparison.Ordinal) ||
                string.Equals(segmento, "DT", StringComparison.Ordinal) ||
                string.Equals(segmento, "DAT", StringComparison.Ordinal) ||
                string.Equals(segmento, "VIG", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static int NormalizarTamanho(ManifestoCampoEcf campo)
    {
        var tamanhos = ExtrairInteiros(campo.Size).ToList();
        if ((CpfTokenPattern().IsMatch(campo.Decimals) ||
             CnpjTokenPattern().IsMatch(campo.Decimals) ||
             NifTokenPattern().IsMatch(campo.Decimals)) &&
            !int.TryParse(
                campo.Decimals,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out _))
        {
            tamanhos.AddRange(ExtrairInteiros(campo.Decimals));
        }

        return tamanhos.Count == 0 ? 0 : tamanhos.Max();
    }

    private static IEnumerable<int> ExtrairInteiros(string valor)
    {
        foreach (Match match in IntegerPattern().Matches(valor))
        {
            if (int.TryParse(
                match.ValueSpan,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int numero))
            {
                yield return numero;
            }
        }
    }

    private static int NormalizarDecimais(string valor)
        => int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out int decimais)
            ? decimais
            : 0;

    private static bool NormalizarObrigatorio(string valor)
    {
        string normalizado = CanonicalFieldName(valor);
        return normalizado is "SIM" or "S";
    }

    internal static string CanonicalFieldName(string valor)
    {
        var resultado = new StringBuilder(valor.Length);
        foreach (char caractere in valor)
        {
            if (!char.IsLetterOrDigit(caractere))
                continue;

            resultado.Append(char.ToUpperInvariant(caractere));
        }

        return resultado.ToString();
    }

    [GeneratedRegex("(?<![A-Z0-9])CPF(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CpfTokenPattern();

    [GeneratedRegex("(?<![A-Z0-9])CNPJ(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CnpjTokenPattern();

    [GeneratedRegex("(?<![A-Z0-9])NIF(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NifTokenPattern();

    [GeneratedRegex("[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex IntegerPattern();

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

    /// <summary>
    /// Matriz semântica derivada de tokens explícitos no nome do campo e de metadados
    /// estruturais, nunca da descrição livre ou do código do registro. Formas exclusivas
    /// exigem o value object forte; formas compostas preservam <see cref="string"/> para não
    /// perder CPF/CNPJ/NIF alternativos.
    /// </summary>
    private enum CampoSemantica
    {
        Generico,
        Data,
        Cpf,
        Cnpj,
        DocumentoComposto,
    }
}
