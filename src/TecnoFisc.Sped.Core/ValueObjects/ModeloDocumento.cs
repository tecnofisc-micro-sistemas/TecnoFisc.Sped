namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Modelo de documento fiscal — Tabela 4.1.1 do Ato COTEPE/ICMS nº 09/2008 e nº 44/2018.
/// Regida pelo EFD ICMS-IPI; referenciada por EFD Contribuições e demais leiautes que
/// escrituram documentos fiscais (campo COD_MOD).
/// </summary>
/// <remarks>
/// O código pode ser alfanumérico (ex.: "1B" Nota Fiscal Avulsa, "2D" Cupom Fiscal,
/// "2E" Cupom Fiscal Bilhete de Passagem, "8B" CT Avulso, "3A"/"3B" Notas de Serviços
/// modelos especiais), portanto não é representável como enum int-backed.
/// </remarks>
public readonly struct ModeloDocumento : IEquatable<ModeloDocumento>
{
    private readonly string? _codigo;

    /// <summary>Código SPED do modelo (campo COD_MOD), ex.: "01", "55", "1B".</summary>
    public string Codigo => _codigo ?? string.Empty;

    /// <summary>Descrição oficial do modelo conforme tabela COTEPE.</summary>
    public string Descricao { get; }

    /// <summary>Modelo numérico do documento (coluna "Modelo" da tabela 4.1.1). Pode ser nulo para documentos sem modelo numérico próprio.</summary>
    public string? ModeloNumerico { get; }

    /// <summary>Registro pai onde o modelo é escriturado no leiaute EFD ICMS-IPI. Informativo; outros leiautes podem usar registros distintos.</summary>
    public string? RegistroPaiIcmsIpi { get; }

    private ModeloDocumento(string codigo, string descricao, string? modeloNumerico, string? registroPaiIcmsIpi)
    {
        _codigo = codigo;
        Descricao = descricao;
        ModeloNumerico = modeloNumerico;
        RegistroPaiIcmsIpi = registroPaiIcmsIpi;
    }

    /// <exception cref="FormatException">Quando o código não corresponde a nenhum modelo conhecido.</exception>
    public static ModeloDocumento Create(ReadOnlySpan<char> codigo)
    {
        if (!TentarCriar(codigo, out var modelo))
            throw new FormatException($"Modelo de documento desconhecido: '{codigo}'.");
        return modelo;
    }

    public static bool TentarCriar(ReadOnlySpan<char> codigo, out ModeloDocumento modelo)
    {
        Span<char> normalizado = stackalloc char[codigo.Length];
        for (int i = 0; i < codigo.Length; i++)
            normalizado[i] = char.ToUpperInvariant(codigo[i]);

        return Catalogo.TryGetValue(new string(normalizado), out modelo);
    }

    /// <summary>Enumera todos os modelos conhecidos da tabela 4.1.1.</summary>
    public static IEnumerable<ModeloDocumento> Todos => Catalogo.Values;

    public override string ToString() => Codigo;

    public bool Equals(ModeloDocumento other)
        => string.Equals(_codigo, other._codigo, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ModeloDocumento m && Equals(m);
    public override int GetHashCode() => _codigo?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(ModeloDocumento left, ModeloDocumento right) => left.Equals(right);
    public static bool operator !=(ModeloDocumento left, ModeloDocumento right) => !left.Equals(right);

    public static implicit operator string(ModeloDocumento modelo) => modelo.ToString();

    private static readonly Dictionary<string, ModeloDocumento> Catalogo = new(StringComparer.Ordinal)
    {
        ["01"] = new("01", "Nota Fiscal", "1/1A", "C100"),
        ["1B"] = new("1B", "Nota Fiscal Avulsa", null, "C100"),
        ["02"] = new("02", "Nota Fiscal de Venda a Consumidor", "2", "C300 ou C350 ou C400 (emissão por ECF)"),
        ["2D"] = new("2D", "Cupom Fiscal", null, "C400"),
        ["2E"] = new("2E", "Cupom Fiscal Bilhete de Passagem", null, "D350 (emissão por ECF)"),
        ["04"] = new("04", "Nota Fiscal de Produtor", "4", "C100"),
        ["06"] = new("06", "Nota Fiscal/Conta de Energia Elétrica", "6", "Aquisição: C500; Fornecimento: C500 ou C600; C700 (Convênio 115/03)"),
        ["07"] = new("07", "Nota Fiscal de Serviço de Transporte", "7", "D100"),
        ["08"] = new("08", "Conhecimento de Transporte Rodoviário de Cargas", "8", "D100"),
        ["8B"] = new("8B", "Conhecimento de Transporte de Cargas Avulso", null, "D100"),
        ["09"] = new("09", "Conhecimento de Transporte Aquaviário de Cargas", "9", "D100"),
        ["10"] = new("10", "Conhecimento Aéreo", "10", "D100"),
        ["11"] = new("11", "Conhecimento de Transporte Ferroviário de Cargas", "11", "D100"),
        ["13"] = new("13", "Bilhete de Passagem Rodoviário", "13", "D300"),
        ["14"] = new("14", "Bilhete de Passagem Aquaviário", "14", "D300"),
        ["15"] = new("15", "Bilhete de Passagem e Nota de Bagagem", "15", "D300"),
        ["16"] = new("16", "Bilhete de Passagem Ferroviário", "16", "D300"),
        ["17"] = new("17", "Despacho de Transporte", "17", null),
        ["18"] = new("18", "Resumo de Movimento Diário", "18", "D400"),
        ["20"] = new("20", "Ordem de Coleta de Cargas", "20", null),
        ["21"] = new("21", "Nota Fiscal de Serviço de Comunicação", "21", "Aquisição: D500; Prestação: D500 ou D600; D695 (Convênio 115/03)"),
        ["22"] = new("22", "Nota Fiscal de Serviço de Telecomunicação", "22", "Aquisição: D500; Prestação: D500 ou D600; D695 (Convênio 115/03)"),
        ["23"] = new("23", "GNRE", "23", null),
        ["24"] = new("24", "Autorização de Carregamento e Transporte", "24", null),
        ["25"] = new("25", "Manifesto de Carga", "25", null),
        ["26"] = new("26", "Conhecimento de Transporte Multimodal de Cargas", "26", "D100"),
        ["27"] = new("27", "Nota Fiscal De Transporte Ferroviário De Carga", "27", "D100"),
        ["28"] = new("28", "Nota Fiscal/Conta de Fornecimento de Gás Canalizado", "28", "C500 ou C600"),
        ["29"] = new("29", "Nota Fiscal/Conta de Fornecimento D'Água Canalizada", "29", "C500 ou C600"),
        ["55"] = new("55", "Nota Fiscal Eletrônica - NF-e", "55", "C100"),
        ["57"] = new("57", "Conhecimento de Transporte Eletrônico - CT-e", "57", "D100"),
        ["59"] = new("59", "Cupom Fiscal Eletrônico - CF-e-SAT", "59", "C800 ou C850"),
        ["60"] = new("60", "Cupom Fiscal Eletrônico CF-e-ECF", "60", "C400"),
        ["62"] = new("62", "Nota Fiscal Fatura Eletrônica de Serviços de Comunicação - NFCom", "62", "D700"),
        ["63"] = new("63", "Bilhete de Passagem Eletrônico - BP-e", "63", "D100"),
        ["65"] = new("65", "Nota Fiscal Eletrônica para Consumidor Final - NFC-e", "65", "C100"),
        ["66"] = new("66", "Nota Fiscal de Energia Elétrica Eletrônica - NF3e", "66", "C500"),
        ["67"] = new("67", "Conhecimento de Transporte Eletrônico Para Outros Serviços - CT-e OS", "67", "D100"),
    };
}
