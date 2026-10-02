using System.Globalization;
using System.Xml;

using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.NFeNFCe.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Desserializa uma NF-e (modelo 55) a partir de um <see cref="XmlReader"/> forward-only,
/// de forma <b>order-independent</b>: o entry point varre o documento e colhe o grupo
/// <c>infNFe</c> e o protocolo (<c>infProt</c>) onde quer que apareçam — o que cobre tanto o
/// XML canônico (<c>nfeProc</c>/<c>NFe</c>) quanto o envelope SERPRO (SOAP →
/// <c>retConsNFeLog</c> → <c>NFeLog</c>), que embaralha a ordem dos elementos e inverte
/// <c>protNFe</c>/<c>NFe</c>. Eventos embutidos no envelope SERPRO são ignorados nesta slice
/// (entram na 14.7).
/// </summary>
/// <remarks>
/// Instância de uso único, criada por chamada de <c>ParserNFe.ReadAsync</c> — não compartilha
/// estado mutável entre threads (o estado é o <see cref="XmlReader"/> da própria chamada).
/// A chave de acesso é sempre validada (é a identidade do documento);
/// <see cref="ParserNFeOptions.ValidateChecksums"/> governa os demais value objects com DV (GTIN).
/// </remarks>
internal sealed partial class NFeXmlReader(XmlReader reader, ParserNFeOptions options)
{
    private readonly XmlReader _r = reader;
    private readonly ParserNFeOptions _options = options;

    private sealed record InfNFe(
        ChaveAcesso Chave, string Versao, Identificacao Ide, Emitente Emit,
        Destinatario? Dest, Total Total, IReadOnlyList<Item> Itens)
    {
    /// <summary>XML: <c>avulsa</c>.</summary>
    public InfNFeAvulsa? Avulsa { get; init; }
    /// <summary>XML: <c>retirada</c>.</summary>
    public LocalRetirada? Retirada { get; init; }
    /// <summary>XML: <c>entrega</c>.</summary>
    public LocalEntrega? Entrega { get; init; }
    /// <summary>XML: <c>autXML</c>.</summary>
    public IReadOnlyList<AutorizadoXml> AutXML { get; init; } = [];
    /// <summary>XML: <c>transp</c>.</summary>
    public Transporte? Transp { get; init; }
    /// <summary>XML: <c>cobr</c>.</summary>
    public Cobranca? Cobr { get; init; }
    /// <summary>XML: <c>pag</c>.</summary>
    public Pagamento? Pag { get; init; }
    /// <summary>XML: <c>infIntermed</c>.</summary>
    public Intermediador? InfIntermed { get; init; }
    /// <summary>XML: <c>infAdic</c>.</summary>
    public InformacoesAdicionais? InfAdic { get; init; }
    /// <summary>XML: <c>exporta</c>.</summary>
    public Exportacao? Exporta { get; init; }
    /// <summary>XML: <c>compra</c>.</summary>
    public Compra? Compra { get; init; }
    /// <summary>XML: <c>cana</c>.</summary>
    public Cana? Cana { get; init; }
    /// <summary>XML: <c>infRespTec</c>.</summary>
    public ResponsavelTecnico? InfRespTec { get; init; }
    /// <summary>XML: <c>infSolicNFF</c>.</summary>
    public InfNFeInfSolicNFF? InfSolicNFF { get; init; }
    /// <summary>XML: <c>agropecuario</c>.</summary>
    public InfNFeAgropecuario? Agropecuario { get; init; }
    /// <summary>XML: <c>infPAA</c>.</summary>
    public InfNFeInfPAA? InfPAA { get; init; }
    }

    /// <summary>Varre o documento e materializa a <see cref="NFe"/> (com protocolo, se houver).</summary>
    public async Task<NFe> ReadAsync(CancellationToken cancellationToken)
    {
        InfNFe? core = null;
        Protocolo? protocolo = null;
        InformacaoSuplementar? suplemento = null;

        if (!await _r.ReadAsync().ConfigureAwait(false))
            throw new FormatException("Documento XML vazio.");

        while (!_r.EOF)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_r.NodeType != XmlNodeType.Element)
            {
                if (!await _r.ReadAsync().ConfigureAwait(false))
                    break;
                continue;
            }

            switch (_r.LocalName)
            {
                case "infNFeSupl":
                    suplemento = await ReadInformacaoSuplementarAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case "infNFe":
                    if (core is null)
                        core = await ReadInfNFeAsync(cancellationToken).ConfigureAwait(false);
                    else
                        throw new FormatException("Múltiplos grupos infNFe na mesma unidade documental.");
                    break; // leitor/Skip já deixou o reader no nó seguinte

                case "infProt":
                    if (protocolo is null)
                        protocolo = await ReadInfProtAsync(cancellationToken).ConfigureAwait(false);
                    else
                        throw new FormatException("Múltiplos protocolos na mesma unidade documental.");
                    break;

                default:
                    // Desce em qualquer envelope (soap/NFeLog/nfeProc/NFe/Signature/eventos)
                    // procurando os nós de interesse.
                    if (!await _r.ReadAsync().ConfigureAwait(false))
                        goto done;
                    break;
            }
        }

    done:
        if (core is null)
            throw new FormatException("Nenhum grupo infNFe encontrado no documento.");

        if (protocolo is not null && protocolo.ChNFe != core.Chave)
            throw new FormatException("Protocolo pertence a outra chave de acesso.");
        if (core.Versao.Length != 0 && core.Versao != "4.00")
            throw new FormatException($"Versão não suportada: {core.Versao}.");
        if (!core.Chave.ToString().AsSpan(20, 2).SequenceEqual(core.Ide.Mod.Codigo.AsSpan()))
            throw new FormatException("Modelo da chave difere do grupo ide.");

        return new NFe
        {
            ChaveAcesso = core.Chave,
            Versao = core.Versao,
            Ide = core.Ide,
            Emit = core.Emit,
            Dest = core.Dest,
            Total = core.Total,
            Itens = core.Itens,
            Protocolo = protocolo,
            InfNFeSupl = suplemento,
            Avulsa = core.Avulsa,
            Retirada = core.Retirada,
            Entrega = core.Entrega,
            AutXML = core.AutXML,
            Transp = core.Transp,
            Cobr = core.Cobr,
            Pag = core.Pag,
            InfIntermed = core.InfIntermed,
            InfAdic = core.InfAdic,
            Exporta = core.Exporta,
            Compra = core.Compra,
            Cana = core.Cana,
            InfRespTec = core.InfRespTec,
            InfSolicNFF = core.InfSolicNFF,
            Agropecuario = core.Agropecuario,
            InfPAA = core.InfPAA,
        };
    }

    private async Task<InfNFe> ReadInfNFeAsync(CancellationToken ct)
    {
        // Id e versao podem vir como atributo (canônico) ou elemento-filho (SERPRO).
        string? id = _r.GetAttribute("Id");
        string? versao = _r.GetAttribute("versao");

        Identificacao? ide = null;
        Emitente? emit = null;
        Destinatario? dest = null;
        Total? total = null;
        var itens = new List<Item>();

        InfNFeAvulsa? extraAvulsa = null;
        LocalRetirada? extraRetirada = null;
        LocalEntrega? extraEntrega = null;
        List<AutorizadoXml>? extraAutXML = null;
        Transporte? extraTransp = null;
        Cobranca? extraCobr = null;
        Pagamento? extraPag = null;
        Intermediador? extraInfIntermed = null;
        InformacoesAdicionais? extraInfAdic = null;
        Exportacao? extraExporta = null;
        Compra? extraCompra = null;
        Cana? extraCana = null;
        ResponsavelTecnico? extraInfRespTec = null;
        InfNFeInfSolicNFF? extraInfSolicNFF = null;
        InfNFeAgropecuario? extraAgropecuario = null;
        InfNFeInfPAA? extraInfPAA = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ide": ide = await ReadIdeAsync(ct).ConfigureAwait(false); return true;
                case "emit": emit = await ReadEmitenteAsync(ct).ConfigureAwait(false); return true;
                case "dest": dest = await ReadDestinatarioAsync(ct).ConfigureAwait(false); return true;
                case "total": total = await ReadTotalAsync(ct).ConfigureAwait(false); return true;
                case "det": itens.Add(await ReadItemAsync(ct).ConfigureAwait(false)); return true;
                // Lê sempre (consome o elemento), mas o atributo homônimo tem precedência se já veio.
                case "Id": { var v = await _r.ReadTextAsync().ConfigureAwait(false); id ??= v; return true; }
                case "versao": { var v = await _r.ReadTextAsync().ConfigureAwait(false); versao ??= v; return true; }
                case "avulsa": extraAvulsa = await ReadInfNFeAvulsaAsync(ct).ConfigureAwait(false); return true;
                case "retirada": extraRetirada = await ReadLocalRetiradaAsync(ct).ConfigureAwait(false); return true;
                case "entrega": extraEntrega = await ReadLocalEntregaAsync(ct).ConfigureAwait(false); return true;
                case "autXML": (extraAutXML ??= []).Add(await ReadAutorizadoXmlAsync(ct).ConfigureAwait(false)); return true;
                case "transp": extraTransp = await ReadTransporteAsync(ct).ConfigureAwait(false); return true;
                case "cobr": extraCobr = await ReadCobrancaAsync(ct).ConfigureAwait(false); return true;
                case "pag": extraPag = await ReadPagamentoAsync(ct).ConfigureAwait(false); return true;
                case "infIntermed": extraInfIntermed = await ReadIntermediadorAsync(ct).ConfigureAwait(false); return true;
                case "infAdic": extraInfAdic = await ReadInformacoesAdicionaisAsync(ct).ConfigureAwait(false); return true;
                case "exporta": extraExporta = await ReadExportacaoAsync(ct).ConfigureAwait(false); return true;
                case "compra": extraCompra = await ReadCompraAsync(ct).ConfigureAwait(false); return true;
                case "cana": extraCana = await ReadCanaAsync(ct).ConfigureAwait(false); return true;
                case "infRespTec": extraInfRespTec = await ReadResponsavelTecnicoAsync(ct).ConfigureAwait(false); return true;
                case "infSolicNFF": extraInfSolicNFF = await ReadInfNFeInfSolicNFFAsync(ct).ConfigureAwait(false); return true;
                case "agropecuario": extraAgropecuario = await ReadInfNFeAgropecuarioAsync(ct).ConfigureAwait(false); return true;
                case "infPAA": extraInfPAA = await ReadInfNFeInfPAAAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new InfNFe(
            ChaveFromId(id),
            versao ?? string.Empty,
            ide ?? throw new FormatException("infNFe sem grupo ide."),
            emit ?? throw new FormatException("infNFe sem grupo emit."),
            dest,
            total ?? throw new FormatException("infNFe sem grupo total."),
            itens) { Avulsa = extraAvulsa, Retirada = extraRetirada, Entrega = extraEntrega, AutXML = (IReadOnlyList<AutorizadoXml>?)extraAutXML ?? [], Transp = extraTransp, Cobr = extraCobr, Pag = extraPag, InfIntermed = extraInfIntermed, InfAdic = extraInfAdic, Exporta = extraExporta, Compra = extraCompra, Cana = extraCana, InfRespTec = extraInfRespTec, InfSolicNFF = extraInfSolicNFF, Agropecuario = extraAgropecuario, InfPAA = extraInfPAA };
    }

    private async Task<Identificacao> ReadIdeAsync(CancellationToken ct)
    {
        int cUF = 0; string cNF = string.Empty; string natOp = string.Empty; ModeloDocumento mod = default;
        int serie = 0; int nNF = 0; DateTimeOffset dhEmi = default; DateTimeOffset? dhSaiEnt = null;
        IndicadorOperacao tpNF = default; int idDest = 0; CodigoMunicipioIbge cMunFG = default;
        int tpImp = 0; TipoEmissao tpEmis = default; int cDV = 0; TipoAmbiente tpAmb = default;
        FinalidadeEmissao finNFe = default; bool indFinal = false; IndicadorPresenca indPres = default;
        IndicadorIntermediador? indIntermed = null; int procEmi = 0; string? verProc = null;

        DateOnly? extraDPrevEntrega = null;
        string? extraCMunFGIBS = null;
        string? extraTpNFDebito = null;
        string? extraTpNFCredito = null;
        string? extraCIndOp = null;
        DateTimeOffset? extraDhCont = null;
        string? extraXJust = null;
        List<ReferenciaNota>? extraNFref = null;
        IdentificacaoGCompraGov? extraGCompraGov = null;
        IdentificacaoGPagAntecipado? extraGPagAntecipado = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cUF": cUF = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cNF": cNF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "natOp": natOp = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "mod": mod = ModeloDocumento.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "serie": serie = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "nNF": nNF = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "dhEmi": dhEmi = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "dhSaiEnt": dhSaiEnt = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpNF": tpNF = ParseEnum<IndicadorOperacao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "idDest": idDest = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cMunFG": cMunFG = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpImp": tpImp = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpEmis": tpEmis = ParseEnum<TipoEmissao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cDV": cDV = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpAmb": tpAmb = ParseEnum<TipoAmbiente>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "finNFe": finNFe = ParseEnum<FinalidadeEmissao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indFinal": indFinal = ParseBool(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indPres": indPres = ParseEnum<IndicadorPresenca>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indIntermed": indIntermed = ParseEnum<IndicadorIntermediador>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "procEmi": procEmi = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "verProc": verProc = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "dPrevEntrega": extraDPrevEntrega = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture); return true;
                case "cMunFGIBS": extraCMunFGIBS = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "tpNFDebito": extraTpNFDebito = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "tpNFCredito": extraTpNFCredito = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cIndOp": extraCIndOp = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "dhCont": extraDhCont = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xJust": extraXJust = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "NFref": (extraNFref ??= []).Add(await ReadReferenciaNotaAsync(ct).ConfigureAwait(false)); return true;
                case "gCompraGov": extraGCompraGov = await ReadIdentificacaoGCompraGovAsync(ct).ConfigureAwait(false); return true;
                case "gPagAntecipado": extraGPagAntecipado = await ReadIdentificacaoGPagAntecipadoAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Identificacao
        { DPrevEntrega = extraDPrevEntrega, CMunFGIBS = extraCMunFGIBS, TpNFDebito = extraTpNFDebito, TpNFCredito = extraTpNFCredito, CIndOp = extraCIndOp, DhCont = extraDhCont, XJust = extraXJust, NFref = (IReadOnlyList<ReferenciaNota>?)extraNFref ?? [], GCompraGov = extraGCompraGov, GPagAntecipado = extraGPagAntecipado, 
            CUF = cUF, CNF = cNF, NatOp = natOp, Mod = mod, Serie = serie, NNF = nNF,
            DhEmi = dhEmi, DhSaiEnt = dhSaiEnt, TpNF = tpNF, IdDest = idDest, CMunFG = cMunFG,
            TpImp = tpImp, TpEmis = tpEmis, CDV = cDV, TpAmb = tpAmb, FinNFe = finNFe,
            IndFinal = indFinal, IndPres = indPres, IndIntermed = indIntermed,
            ProcEmi = procEmi, VerProc = verProc,
        };
    }

    private async Task<Emitente> ReadEmitenteAsync(CancellationToken ct)
    {
        Cnpj? cnpj = null; Cpf? cpf = null; string xNome = string.Empty; string? xFant = null;
        Endereco? ender = null; InscricaoEstadual? ie = null; string? iest = null, im = null, cnae = null;
        int crt = 0;

        string? extraISUFEmit = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": cnpj = Cnpj.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CPF": cpf = Cpf.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xNome": xNome = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xFant": xFant = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "enderEmit": ender = await ReadEnderecoAsync(ct).ConfigureAwait(false); return true;
                case "IE": ie = InscricaoEstadual.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "IEST": iest = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "IM": im = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNAE": cnae = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CRT": crt = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "ISUFEmit": extraISUFEmit = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Emitente
        { ISUFEmit = extraISUFEmit, 
            CNPJ = cnpj, CPF = cpf, XNome = xNome, XFant = xFant,
            EnderEmit = ender ?? throw new FormatException("emit sem grupo enderEmit."),
            IE = ie, IEST = iest, IM = im, CNAE = cnae, CRT = crt,
        };
    }

    private async Task<Destinatario> ReadDestinatarioAsync(CancellationToken ct)
    {
        Cnpj? cnpj = null; Cpf? cpf = null; string? idEstrangeiro = null, xNome = null, email = null;
        Endereco? ender = null; int? indIEDest = null; InscricaoEstadual? ie = null;

        string? extraISUF = null;
        string? extraIM = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": cnpj = Cnpj.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CPF": cpf = Cpf.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "idEstrangeiro": idEstrangeiro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xNome": xNome = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "enderDest": ender = await ReadEnderecoAsync(ct).ConfigureAwait(false); return true;
                case "indIEDest": indIEDest = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "IE": ie = InscricaoEstadual.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "email": email = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "ISUF": extraISUF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "IM": extraIM = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Destinatario
        { ISUF = extraISUF, IM = extraIM, 
            CNPJ = cnpj, CPF = cpf, IdEstrangeiro = idEstrangeiro, XNome = xNome,
            EnderDest = ender, IndIEDest = indIEDest, IE = ie, Email = email,
        };
    }

    private async Task<Endereco> ReadEnderecoAsync(CancellationToken ct)
    {
        string xLgr = string.Empty, nro = string.Empty, xBairro = string.Empty, xMun = string.Empty, uf = string.Empty;
        string? xCpl = null, cep = null, xPais = null, fone = null;
        CodigoMunicipioIbge cMun = default; int? cPais = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xLgr": xLgr = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nro": nro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xCpl": xCpl = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xBairro": xBairro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cMun": cMun = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xMun": xMun = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "UF": uf = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CEP": cep = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cPais": cPais = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xPais": xPais = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "fone": fone = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Endereco
        {
            XLgr = xLgr, Nro = nro, XCpl = xCpl, XBairro = xBairro, CMun = cMun,
            XMun = xMun, UF = uf, CEP = cep, CPais = cPais, XPais = xPais, Fone = fone,
        };
    }

    private async Task<Total> ReadTotalAsync(CancellationToken ct)
    {
        TotalIcms? icmsTot = null;

        TotalIssqn? extraISSQNtot = null;
        RetencoesTributos? extraRetTrib = null;
        TotalIS? extraISTot = null;
        TotalIBSCBS? extraIBSCBSTot = null;
        decimal? extraVNFTot = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ICMSTot": icmsTot = await ReadTotalIcmsAsync(ct).ConfigureAwait(false); return true;
                case "ISSQNtot": extraISSQNtot = await ReadTotalIssqnAsync(ct).ConfigureAwait(false); return true;
                case "retTrib": extraRetTrib = await ReadRetencoesTributosAsync(ct).ConfigureAwait(false); return true;
                case "ISTot": extraISTot = await ReadTotalISAsync(ct).ConfigureAwait(false); return true;
                case "IBSCBSTot": extraIBSCBSTot = await ReadTotalIBSCBSAsync(ct).ConfigureAwait(false); return true;
                case "vNFTot": extraVNFTot = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Total { ISSQNtot = extraISSQNtot, RetTrib = extraRetTrib, ISTot = extraISTot, IBSCBSTot = extraIBSCBSTot, VNFTot = extraVNFTot,  ICMSTot = icmsTot ?? throw new FormatException("total sem grupo ICMSTot.") };
    }

    private async Task<TotalIcms> ReadTotalIcmsAsync(CancellationToken ct)
    {
        decimal vBC = 0, vICMS = 0, vICMSDeson = 0, vFCP = 0, vBCST = 0, vST = 0, vFCPST = 0,
            vFCPSTRet = 0, vProd = 0, vFrete = 0, vSeg = 0, vDesc = 0, vII = 0, vIPI = 0,
            vIPIDevol = 0, vPIS = 0, vCOFINS = 0, vOutro = 0, vNF = 0;

        decimal? extraVFCPUFDest = null;
        decimal? extraVICMSUFDest = null;
        decimal? extraVICMSUFRemet = null;
        decimal? extraQBCMono = null;
        decimal? extraVICMSMono = null;
        decimal? extraQBCMonoReten = null;
        decimal? extraVICMSMonoReten = null;
        decimal? extraQBCMonoRet = null;
        decimal? extraVICMSMonoRet = null;
        decimal? extraVTotTrib = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vST": vST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPSTRet": vFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vProd": vProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFrete": vFrete = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vSeg": vSeg = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vDesc": vDesc = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vII": vII = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIPI": vIPI = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIPIDevol": vIPIDevol = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vPIS": vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCOFINS": vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vOutro": vOutro = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vNF": vNF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPUFDest": extraVFCPUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSUFDest": extraVICMSUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSUFRemet": extraVICMSUFRemet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qBCMono": extraQBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMono": extraVICMSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qBCMonoReten": extraQBCMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMonoReten": extraVICMSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qBCMonoRet": extraQBCMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMonoRet": extraVICMSMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTotTrib": extraVTotTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new TotalIcms
        { VFCPUFDest = extraVFCPUFDest, VICMSUFDest = extraVICMSUFDest, VICMSUFRemet = extraVICMSUFRemet, QBCMono = extraQBCMono, VICMSMono = extraVICMSMono, QBCMonoReten = extraQBCMonoReten, VICMSMonoReten = extraVICMSMonoReten, QBCMonoRet = extraQBCMonoRet, VICMSMonoRet = extraVICMSMonoRet, VTotTrib = extraVTotTrib, 
            VBC = vBC, VICMS = vICMS, VICMSDeson = vICMSDeson, VFCP = vFCP, VBCST = vBCST,
            VST = vST, VFCPST = vFCPST, VFCPSTRet = vFCPSTRet, VProd = vProd, VFrete = vFrete,
            VSeg = vSeg, VDesc = vDesc, VII = vII, VIPI = vIPI, VIPIDevol = vIPIDevol,
            VPIS = vPIS, VCOFINS = vCOFINS, VOutro = vOutro, VNF = vNF,
        };
    }

    private async Task<Item> ReadItemAsync(CancellationToken ct)
    {
        // nItem vem como atributo (canônico) ou elemento-filho (SERPRO).
        string? nItemAttr = _r.GetAttribute("nItem");
        int nItem = nItemAttr is null ? 0 : ParseInt(nItemAttr);

        Produto? prod = null;
        var imposto = new Imposto();

        ItemImpostoDevol? extraImpostoDevol = null;
        string? extraInfAdProd = null;
        ItemObsItem? extraObsItem = null;
        decimal? extraVItem = null;
        ItemDFeReferenciado? extraDFeReferenciado = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nItem": nItem = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "prod": prod = await ReadProdutoAsync(ct).ConfigureAwait(false); return true;
                case "imposto": imposto = await ReadImpostoAsync(ct).ConfigureAwait(false); return true;
                case "impostoDevol": extraImpostoDevol = await ReadItemImpostoDevolAsync(ct).ConfigureAwait(false); return true;
                case "infAdProd": extraInfAdProd = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "obsItem": extraObsItem = await ReadItemObsItemAsync(ct).ConfigureAwait(false); return true;
                case "vItem": extraVItem = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "DFeReferenciado": extraDFeReferenciado = await ReadItemDFeReferenciadoAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Item
        { ImpostoDevol = extraImpostoDevol, InfAdProd = extraInfAdProd, ObsItem = extraObsItem, VItem = extraVItem, DFeReferenciado = extraDFeReferenciado, 
            NItem = nItem,
            Prod = prod ?? throw new FormatException($"det (nItem={nItem}) sem grupo prod."),
            Imposto = imposto,
        };
    }

    private async Task<Produto> ReadProdutoAsync(CancellationToken ct)
    {
        string cProd = string.Empty, xProd = string.Empty, uCom = string.Empty, uTrib = string.Empty;
        string? extipi = null;
        Gtin cEAN = default, cEANTrib = default; Ncm ncm = default; Cest? cest = null; Cfop cfop = default;
        decimal qCom = 0, vUnCom = 0, vProd = 0, qTrib = 0, vUnTrib = 0; bool indTot = false;

        string? extraCBarra = null;
        List<string>? extraNVE = null;
        string? extraIndEscala = null;
        string? extraCNPJFab = null;
        string? extraCBenef = null;
        List<ProdutoGCred>? extraGCred = null;
        string? extraTpCredPresIBSZFM = null;
        string? extraCBarraTrib = null;
        decimal? extraVFrete = null;
        decimal? extraVSeg = null;
        decimal? extraVDesc = null;
        decimal? extraVOutro = null;
        string? extraIndBemMovelUsado = null;
        List<DeclaracaoImportacao>? extraDI = null;
        List<DetalheExportacao>? extraDetExport = null;
        string? extraXPed = null;
        string? extraNItemPed = null;
        string? extraNFCI = null;
        List<Rastreabilidade>? extraRastro = null;
        ProdutoInfProdNFF? extraInfProdNFF = null;
        ProdutoInfProdEmb? extraInfProdEmb = null;
        VeiculoProduto? extraVeicProd = null;
        Medicamento? extraMed = null;
        List<Armamento>? extraArma = null;
        Combustivel? extraComb = null;
        string? extraNRECOPI = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cProd": cProd = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cEAN": cEAN = ParseGtin(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xProd": xProd = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "NCM": ncm = Ncm.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CEST": cest = Cest.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "EXTIPI": extipi = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CFOP": cfop = Cfop.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "uCom": uCom = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qCom": qCom = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vUnCom": vUnCom = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vProd": vProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cEANTrib": cEANTrib = ParseGtin(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "uTrib": uTrib = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qTrib": qTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vUnTrib": vUnTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indTot": indTot = ParseBool(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cBarra": extraCBarra = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "NVE": (extraNVE ??= []).Add(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indEscala": extraIndEscala = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNPJFab": extraCNPJFab = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cBenef": extraCBenef = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "gCred": (extraGCred ??= []).Add(await ReadProdutoGCredAsync(ct).ConfigureAwait(false)); return true;
                case "tpCredPresIBSZFM": extraTpCredPresIBSZFM = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cBarraTrib": extraCBarraTrib = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vFrete": extraVFrete = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vSeg": extraVSeg = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vDesc": extraVDesc = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vOutro": extraVOutro = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indBemMovelUsado": extraIndBemMovelUsado = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "DI": (extraDI ??= []).Add(await ReadDeclaracaoImportacaoAsync(ct).ConfigureAwait(false)); return true;
                case "detExport": (extraDetExport ??= []).Add(await ReadDetalheExportacaoAsync(ct).ConfigureAwait(false)); return true;
                case "xPed": extraXPed = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nItemPed": extraNItemPed = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nFCI": extraNFCI = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "rastro": (extraRastro ??= []).Add(await ReadRastreabilidadeAsync(ct).ConfigureAwait(false)); return true;
                case "infProdNFF": extraInfProdNFF = await ReadProdutoInfProdNFFAsync(ct).ConfigureAwait(false); return true;
                case "infProdEmb": extraInfProdEmb = await ReadProdutoInfProdEmbAsync(ct).ConfigureAwait(false); return true;
                case "veicProd": extraVeicProd = await ReadVeiculoProdutoAsync(ct).ConfigureAwait(false); return true;
                case "med": extraMed = await ReadMedicamentoAsync(ct).ConfigureAwait(false); return true;
                case "arma": (extraArma ??= []).Add(await ReadArmamentoAsync(ct).ConfigureAwait(false)); return true;
                case "comb": extraComb = await ReadCombustivelAsync(ct).ConfigureAwait(false); return true;
                case "nRECOPI": extraNRECOPI = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Produto
        { CBarra = extraCBarra, NVE = (IReadOnlyList<string>?)extraNVE ?? [], IndEscala = extraIndEscala, CNPJFab = extraCNPJFab, CBenef = extraCBenef, GCred = (IReadOnlyList<ProdutoGCred>?)extraGCred ?? [], TpCredPresIBSZFM = extraTpCredPresIBSZFM, CBarraTrib = extraCBarraTrib, VFrete = extraVFrete, VSeg = extraVSeg, VDesc = extraVDesc, VOutro = extraVOutro, IndBemMovelUsado = extraIndBemMovelUsado, DI = (IReadOnlyList<DeclaracaoImportacao>?)extraDI ?? [], DetExport = (IReadOnlyList<DetalheExportacao>?)extraDetExport ?? [], XPed = extraXPed, NItemPed = extraNItemPed, NFCI = extraNFCI, Rastro = (IReadOnlyList<Rastreabilidade>?)extraRastro ?? [], InfProdNFF = extraInfProdNFF, InfProdEmb = extraInfProdEmb, VeicProd = extraVeicProd, Med = extraMed, Arma = (IReadOnlyList<Armamento>?)extraArma ?? [], Comb = extraComb, NRECOPI = extraNRECOPI, 
            CProd = cProd, CEAN = cEAN, XProd = xProd, NCM = ncm, CEST = cest, EXTIPI = extipi,
            CFOP = cfop, UCom = uCom, QCom = qCom, VUnCom = vUnCom, VProd = vProd,
            CEANTrib = cEANTrib, UTrib = uTrib, QTrib = qTrib, VUnTrib = vUnTrib, IndTot = indTot,
        };
    }

    private async Task<Imposto> ReadImpostoAsync(CancellationToken ct)
    {
        Icms? icms = null;
        Ipi? ipi = null;
        Pis? pis = null;
        PisSt? pisSt = null;
        Cofins? cofins = null;
        CofinsSt? cofinsSt = null;
        Ii? ii = null;
        Issqn? issqn = null;
        decimal? vTotTrib = null;

        IcmsUfDest? extraICMSUFDest = null;
        ImpostoSeletivo? extraIS = null;
        TributacaoIBSCBS? extraIBSCBS = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ICMS": icms = await ReadIcmsAsync(ct).ConfigureAwait(false); return true;
                case "IPI": ipi = await ReadIpiAsync(ct).ConfigureAwait(false); return true;
                case "PIS": pis = await ReadPisAsync(ct).ConfigureAwait(false); return true;
                case "PISST": pisSt = await ReadPisStAsync(ct).ConfigureAwait(false); return true;
                case "COFINS": cofins = await ReadCofinsAsync(ct).ConfigureAwait(false); return true;
                case "COFINSST": cofinsSt = await ReadCofinsStAsync(ct).ConfigureAwait(false); return true;
                case "II": ii = await ReadIiAsync(ct).ConfigureAwait(false); return true;
                case "ISSQN": issqn = await ReadIssqnAsync(ct).ConfigureAwait(false); return true;
                case "vTotTrib": vTotTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "ICMSUFDest": extraICMSUFDest = await ReadIcmsUfDestAsync(ct).ConfigureAwait(false); return true;
                case "IS": extraIS = await ReadImpostoSeletivoAsync(ct).ConfigureAwait(false); return true;
                case "IBSCBS": extraIBSCBS = await ReadTributacaoIBSCBSAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Imposto { ICMSUFDest = extraICMSUFDest, IS = extraIS, IBSCBS = extraIBSCBS,  Icms = icms, Ipi = ipi, Pis = pis, PisSt = pisSt, Cofins = cofins, CofinsSt = cofinsSt, Ii = ii, Issqn = issqn, VTotTrib = vTotTrib };
    }

    private async Task<Protocolo> ReadInfProtAsync(CancellationToken ct)
    {
        TipoAmbiente tpAmb = default; string? verAplic = null; ChaveAcesso chNFe = default; bool chSet = false;
        DateTimeOffset dhRecbto = default; string? nProt = null, digVal = null, xMotivo = null; int cStat = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpAmb": tpAmb = ParseEnum<TipoAmbiente>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "verAplic": verAplic = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "chNFe": chNFe = ChaveAcesso.Create(await _r.ReadTextAsync().ConfigureAwait(false)); chSet = true; return true;
                case "dhRecbto": dhRecbto = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "nProt": nProt = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "digVal": digVal = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cStat": cStat = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xMotivo": xMotivo = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                // No SERPRO, o protocolo traz o nº como elemento <Id> (não é a chave): consome e descarta.
                case "Id": await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Protocolo
        {
            TpAmb = tpAmb, VerAplic = verAplic,
            ChNFe = chSet ? chNFe : throw new FormatException("infProt sem chNFe."),
            DhRecbto = dhRecbto, NProt = nProt, DigVal = digVal, CStat = cStat, XMotivo = xMotivo,
        };
    }

    /// <summary>Converte o atributo/elemento <c>Id</c> ("NFe" + 44 dígitos) em <see cref="ChaveAcesso"/>.</summary>
    private static ChaveAcesso ChaveFromId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new FormatException("infNFe sem atributo/elemento Id — chave de acesso indisponível.");

        ReadOnlySpan<char> s = id.AsSpan().Trim();
        if (s.StartsWith("NFe", StringComparison.OrdinalIgnoreCase))
            s = s[3..];

        return ChaveAcesso.Create(s);
    }

    /// <summary>Constrói um <see cref="Gtin"/> respeitando <see cref="ParserNFeOptions.ValidateChecksums"/>.</summary>
    private Gtin ParseGtin(string valor)
        => _options.ValidateChecksums
            ? Gtin.Create(valor)
            : Gtin.TentarCriar(valor, out var g) ? g : default;

    /// <summary>
    /// Combina <c>orig</c> e o <c>CST</c> de 2 dígitos da NF-e na forma canônica SPED/Ato COTEPE
    /// de 3 dígitos (origem + CST) do value object <see cref="Cst"/> do Core.
    /// </summary>
    private static Cst CombinarCst(OrigemMercadoria orig, string cstTributacao)
        => Cst.Create(string.Concat(((int)orig).ToString(CultureInfo.InvariantCulture), cstTributacao), TipoTributo.Icms);

    private static int ParseInt(string s) => int.Parse(s, CultureInfo.InvariantCulture);

    private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);

    private static decimal ParseDecimal(string s) => decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseDate(string s)
        => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static bool ParseBool(string s) => s == "1";

    private static T ParseEnum<T>(string s) where T : struct, Enum
        => (T)Enum.ToObject(typeof(T), ParseInt(s));
}

