namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Gênero do item de mercadoria/serviço — Tabela 4.2.1 do Ato COTEPE/ICMS nº 09/2008
/// e nº 44/2018. Corresponde aos Capítulos da NCM acrescidos do código "00 - Serviço".
/// Regida pelo EFD ICMS-IPI; referenciada por todos os leiautes que escrituram itens
/// (campo COD_GEN do Registro 0200).
/// </summary>
public readonly struct GeneroItem : IEquatable<GeneroItem>
{
    public const int Tamanho = 2;

    private readonly string? _codigo;

    /// <summary>Código de duas posições (00–99).</summary>
    public string Codigo => _codigo ?? string.Empty;

    /// <summary>Descrição oficial do capítulo NCM ou serviço.</summary>
    public string Descricao { get; }

    private GeneroItem(string codigo, string descricao)
    {
        _codigo = codigo;
        Descricao = descricao;
    }

    /// <exception cref="FormatException">Quando o código não está entre 00 e 99 ou é desconhecido.</exception>
    public static GeneroItem Create(ReadOnlySpan<char> codigo)
    {
        if (!TentarCriar(codigo, out var genero))
            throw new FormatException($"Gênero de item desconhecido: '{codigo}'.");
        return genero;
    }

    public static bool TentarCriar(ReadOnlySpan<char> codigo, out GeneroItem genero)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(codigo, digitos))
        {
            genero = default;
            return false;
        }

        return Catalogo.TryGetValue(new string(digitos), out genero);
    }

    /// <summary>Enumera todos os gêneros conhecidos da tabela 4.2.1.</summary>
    public static IEnumerable<GeneroItem> Todos => Catalogo.Values;

    public override string ToString() => Codigo;

    public bool Equals(GeneroItem other)
        => string.Equals(_codigo, other._codigo, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is GeneroItem g && Equals(g);
    public override int GetHashCode() => _codigo?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(GeneroItem left, GeneroItem right) => left.Equals(right);
    public static bool operator !=(GeneroItem left, GeneroItem right) => !left.Equals(right);

    public static implicit operator string(GeneroItem genero) => genero.ToString();

    private static readonly Dictionary<string, GeneroItem> Catalogo = new(StringComparer.Ordinal)
    {
        ["00"] = new("00", "Serviço"),
        ["01"] = new("01", "Animais vivos"),
        ["02"] = new("02", "Carnes e miudezas, comestíveis"),
        ["03"] = new("03", "Peixes e crustáceos, moluscos e os outros invertebrados aquáticos"),
        ["04"] = new("04", "Leite e laticínios; ovos de aves; mel natural; produtos comestíveis de origem animal, não especificados nem compreendidos em outros Capítulos da TIPI"),
        ["05"] = new("05", "Outros produtos de origem animal, não especificados nem compreendidos em outros Capítulos da TIPI"),
        ["06"] = new("06", "Plantas vivas e produtos de floricultura"),
        ["07"] = new("07", "Produtos hortícolas, plantas, raízes e tubérculos, comestíveis"),
        ["08"] = new("08", "Frutas; cascas de cítricos e de melões"),
        ["09"] = new("09", "Café, chá, mate e especiarias"),
        ["10"] = new("10", "Cereais"),
        ["11"] = new("11", "Produtos da indústria de moagem; malte; amidos e féculas; inulina; glúten de trigo"),
        ["12"] = new("12", "Sementes e frutos oleaginosos; grãos, sementes e frutos diversos; plantas industriais ou medicinais; palha e forragem"),
        ["13"] = new("13", "Gomas, resinas e outros sucos e extratos vegetais"),
        ["14"] = new("14", "Matérias para entrançar e outros produtos de origem vegetal, não especificadas nem compreendidas em outros Capítulos da NCM"),
        ["15"] = new("15", "Gorduras e óleos animais ou vegetais; produtos da sua dissociação; gorduras alimentares elaboradas; ceras de origem animal ou vegetal"),
        ["16"] = new("16", "Preparações de carne, de peixes ou de crustáceos, de moluscos ou de outros invertebrados aquáticos"),
        ["17"] = new("17", "Açúcares e produtos de confeitaria"),
        ["18"] = new("18", "Cacau e suas preparações"),
        ["19"] = new("19", "Preparações à base de cereais, farinhas, amidos, féculas ou de leite; produtos de pastelaria"),
        ["20"] = new("20", "Preparações de produtos hortícolas, de frutas ou de outras partes de plantas"),
        ["21"] = new("21", "Preparações alimentícias diversas"),
        ["22"] = new("22", "Bebidas, líquidos alcoólicos e vinagres"),
        ["23"] = new("23", "Resíduos e desperdícios das indústrias alimentares; alimentos preparados para animais"),
        ["24"] = new("24", "Fumo (tabaco) e seus sucedâneos, manufaturados"),
        ["25"] = new("25", "Sal; enxofre; terras e pedras; gesso, cal e cimento"),
        ["26"] = new("26", "Minérios, escórias e cinzas"),
        ["27"] = new("27", "Combustíveis minerais, óleos minerais e produtos de sua destilação; matérias betuminosas; ceras minerais"),
        ["28"] = new("28", "Produtos químicos inorgânicos; compostos inorgânicos ou orgânicos de metais preciosos, de elementos radioativos, de metais das terras raras ou de isótopos"),
        ["29"] = new("29", "Produtos químicos orgânicos"),
        ["30"] = new("30", "Produtos farmacêuticos"),
        ["31"] = new("31", "Adubos ou fertilizantes"),
        ["32"] = new("32", "Extratos tanantes e tintoriais; taninos e seus derivados; pigmentos e outras matérias corantes, tintas e vernizes, másticos; tintas de escrever"),
        ["33"] = new("33", "Óleos essenciais e resinóides; produtos de perfumaria ou de toucador preparados e preparações cosméticas"),
        ["34"] = new("34", "Sabões, agentes orgânicos de superfície, preparações para lavagem, preparações lubrificantes, ceras artificiais, ceras preparadas, produtos de conservação e limpeza, velas e artigos semelhantes, massas ou pastas para modelar, \"ceras para dentistas\" e composições para dentistas à base de gesso"),
        ["35"] = new("35", "Matérias albuminóides; produtos à base de amidos ou de féculas modificados; colas; enzimas"),
        ["36"] = new("36", "Pólvoras e explosivos; artigos de pirotecnia; fósforos; ligas pirofóricas; matérias inflamáveis"),
        ["37"] = new("37", "Produtos para fotografia e cinematografia"),
        ["38"] = new("38", "Produtos diversos das indústrias químicas"),
        ["39"] = new("39", "Plásticos e suas obras"),
        ["40"] = new("40", "Borracha e suas obras"),
        ["41"] = new("41", "Peles, exceto a peleteria (peles com pêlo*), e couros"),
        ["42"] = new("42", "Obras de couro; artigos de correeiro ou de seleiro; artigos de viagem, bolsas e artefatos semelhantes; obras de tripa"),
        ["43"] = new("43", "Peleteria (peles com pêlo*) e suas obras; peleteria (peles com pêlo*) artificial"),
        ["44"] = new("44", "Madeira, carvão vegetal e obras de madeira"),
        ["45"] = new("45", "Cortiça e suas obras"),
        ["46"] = new("46", "Obras de espartaria ou de cestaria"),
        ["47"] = new("47", "Pastas de madeira ou de outras matérias fibrosas celulósicas; papel ou cartão de reciclar (desperdícios e aparas)"),
        ["48"] = new("48", "Papel e cartão; obras de pasta de celulose, de papel ou de cartão"),
        ["49"] = new("49", "Livros, jornais, gravuras e outros produtos das indústrias gráficas; textos manuscritos ou datilografados, planos e plantas"),
        ["50"] = new("50", "Seda"),
        ["51"] = new("51", "Lã e pêlos finos ou grosseiros; fios e tecidos de crina"),
        ["52"] = new("52", "Algodão"),
        ["53"] = new("53", "Outras fibras têxteis vegetais; fios de papel e tecido de fios de papel"),
        ["54"] = new("54", "Filamentos sintéticos ou artificiais"),
        ["55"] = new("55", "Fibras sintéticas ou artificiais, descontínuas"),
        ["56"] = new("56", "Pastas (\"ouates\"), feltros e falsos tecidos; fios especiais; cordéis, cordas e cabos; artigos de cordoaria"),
        ["57"] = new("57", "Tapetes e outros revestimentos para pavimentos, de matérias têxteis"),
        ["58"] = new("58", "Tecidos especiais; tecidos tufados; rendas; tapeçarias; passamanarias; bordados"),
        ["59"] = new("59", "Tecidos impregnados, revestidos, recobertos ou estratificados; artigos para usos técnicos de matérias têxteis"),
        ["60"] = new("60", "Tecidos de malha"),
        ["61"] = new("61", "Vestuário e seus acessórios, de malha"),
        ["62"] = new("62", "Vestuário e seus acessórios, exceto de malha"),
        ["63"] = new("63", "Outros artefatos têxteis confeccionados; sortidos; artefatos de matérias têxteis, calçados, chapéus e artefatos de uso semelhante, usados; trapos"),
        ["64"] = new("64", "Calçados, polainas e artefatos semelhantes, e suas partes"),
        ["65"] = new("65", "Chapéus e artefatos de uso semelhante, e suas partes"),
        ["66"] = new("66", "Guarda-chuvas, sombrinhas, guarda-sóis, bengalas, bengalas-assentos, chicotes, e suas partes"),
        ["67"] = new("67", "Penas e penugem preparadas, e suas obras; flores artificiais; obras de cabelo"),
        ["68"] = new("68", "Obras de pedra, gesso, cimento, amianto, mica ou de matérias semelhantes"),
        ["69"] = new("69", "Produtos cerâmicos"),
        ["70"] = new("70", "Vidro e suas obras"),
        ["71"] = new("71", "Pérolas naturais ou cultivadas, pedras preciosas ou semipreciosas e semelhantes, metais preciosos, metais folheados ou chapeados de metais preciosos, e suas obras; bijuterias; moedas"),
        ["72"] = new("72", "Ferro fundido, ferro e aço"),
        ["73"] = new("73", "Obras de ferro fundido, ferro ou aço"),
        ["74"] = new("74", "Cobre e suas obras"),
        ["75"] = new("75", "Níquel e suas obras"),
        ["76"] = new("76", "Alumínio e suas obras"),
        ["77"] = new("77", "(Reservado para uma eventual utilização futura no SH)"),
        ["78"] = new("78", "Chumbo e suas obras"),
        ["79"] = new("79", "Zinco e suas obras"),
        ["80"] = new("80", "Estanho e suas obras"),
        ["81"] = new("81", "Outros metais comuns; ceramais (\"cermets\"); obras dessas matérias"),
        ["82"] = new("82", "Ferramentas, artefatos de cutelaria e talheres, e suas partes, de metais comuns"),
        ["83"] = new("83", "Obras diversas de metais comuns"),
        ["84"] = new("84", "Reatores nucleares, caldeiras, máquinas, aparelhos e instrumentos mecânicos, e suas partes"),
        ["85"] = new("85", "Máquinas, aparelhos e materiais elétricos, e suas partes; aparelhos de gravação ou de reprodução de som, aparelhos de gravação ou de reprodução de imagens e de som em televisão, e suas partes e acessórios"),
        ["86"] = new("86", "Veículos e material para vias férreas ou semelhantes, e suas partes; aparelhos mecânicos (incluídos os eletromecânicos) de sinalização para vias de comunicação"),
        ["87"] = new("87", "Veículos automóveis, tratores, ciclos e outros veículos terrestres, suas partes e acessórios"),
        ["88"] = new("88", "Aeronaves e aparelhos espaciais, e suas partes"),
        ["89"] = new("89", "Embarcações e estruturas flutuantes"),
        ["90"] = new("90", "Instrumentos e aparelhos de óptica, fotografia ou cinematografia, medida, controle ou de precisão; instrumentos e aparelhos médico-cirúrgicos; suas partes e acessórios"),
        ["91"] = new("91", "Aparelhos de relojoaria e suas partes"),
        ["92"] = new("92", "Instrumentos musicais, suas partes e acessórios"),
        ["93"] = new("93", "Armas e munições; suas partes e acessórios"),
        ["94"] = new("94", "Móveis, mobiliário médico-cirúrgico; colchões; iluminação e construção pré-fabricadas"),
        ["95"] = new("95", "Brinquedos, jogos, artigos para divertimento ou para esporte; suas partes e acessórios"),
        ["96"] = new("96", "Obras diversas"),
        ["97"] = new("97", "Objetos de arte, de coleção e antiguidades"),
        ["98"] = new("98", "(Reservado para usos especiais pelas Partes Contratantes)"),
        ["99"] = new("99", "Operações especiais (utilizado exclusivamente pelo Brasil para classificar operações especiais na exportação)"),
    };
}
