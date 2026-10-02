using System.Globalization;
using System.Text;
using System.Xml;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed class EventoXmlReader(XmlReader reader, ParserNFeOptions options)
{
    public async Task<Evento> ReadAsync(CancellationToken ct)
    {
        int rootDepth = reader.Depth;
        if (reader.IsEmptyElement)
            throw new FormatException("Evento vazio.");
        Header? request = null, response = null;
        string? rootVersion = reader.GetAttribute("versao"), version = null, returnVersion = null;
        int requestDepth = -1, returnDepth = -1;
        bool isReturn = false;
        while (!reader.EOF)
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == rootDepth)
                break;
            if (reader.NodeType == XmlNodeType.Element && !DocumentoXmlReader.IsFiscal(reader))
            {
                await reader.SkipAsync().ConfigureAwait(false);
                continue;
            }
            if (reader.NodeType == XmlNodeType.Element && DocumentoXmlReader.IsFiscal(reader))
            {
                if (reader.LocalName is "evento" or "eventoNFe")
                { isReturn = false; requestDepth = reader.Depth; version = reader.GetAttribute("versao"); }
                if (reader.LocalName == "retEvento")
                { isReturn = true; returnDepth = reader.Depth; returnVersion = reader.GetAttribute("versao"); }
                if (reader.LocalName == "versao")
                {
                    int depth = reader.Depth;
                    string value = await reader.ReadTextAsync().ConfigureAwait(false);
                    if (isReturn && depth == returnDepth + 1)
                        returnVersion ??= value;
                    else if (!isReturn && depth == requestDepth + 1)
                        version ??= value;
                    else if (depth == rootDepth + 1)
                        rootVersion ??= value;
                    continue;
                }
                if (reader.LocalName == "infEvento")
                {
                    var header = await ReadHeaderAsync(ct).ConfigureAwait(false);
                    if (isReturn)
                    {
                        if (response is not null)
                            throw new FormatException("Múltiplos retornos no evento.");
                        response = header;
                    }
                    else
                    {
                        if (request is not null)
                            throw new FormatException("Múltiplas solicitações no evento.");
                        request = header;
                    }
                    continue;
                }
            }
            if (!await reader.ReadAsync().ConfigureAwait(false))
                break;
        }
        if (request is null || request.Type is null)
            throw new FormatException("Evento sem infEvento/tpEvento.");
        if (response is not null && (response.Key is not null && response.Key != request.Key
            || response.Type is not null && response.Type != request.Type || response.Sequence is not null && response.Sequence != request.Sequence))
            throw new FormatException("Retorno não corresponde à chave, tipo ou sequência do evento.");
        if (request.Key is null && request.Type is not (>= 610500 and <= 610615))
            throw new FormatException("Evento sem chNFe.");
        if (request.Type == 110111 && request.Sequence is null)
            throw new FormatException("Cancelamento sem nSeqEvento.");
        Evento result = request.Type == 110111
            ? new EventoCancelamento { ChaveAcesso = default, TpEvento = 110111, NProt = Detail(request.Details, "nProt"), XJust = Detail(request.Details, "xJust") }
            : new EventoGenerico { ChaveAcesso = default, TpEvento = request.Type.Value, Detalhes = request.Details };
        return result with
        {
            ChaveAcesso = request.Key ?? default,
            TpEvento = request.Type.Value,
            NSeqEvento = request.Sequence,
            DhEvento = request.Date,
            Id = request.Id,
            Versao = version ?? rootVersion,
            VerEvento = request.EventVersion,
            VersaoDetalhes = request.Details?.Atributos.FirstOrDefault(a => a.Nome == "versao" && a.Namespace.Length == 0)?.Valor
                ?? Detail(request.Details, "versao"),
            COrgao = request.Orgao,
            TpAmb = request.Environment,
            CNPJ = request.Cnpj,
            CPF = request.Cpf,
            DescEvento = Detail(request.Details, "descEvento"),
            Retorno = response is null ? null : new RetornoEvento
            {
                Versao = returnVersion,
                ChaveAcesso = response.Key,
                TpEvento = response.Type,
                NSeqEvento = response.Sequence,
                CStat = response.Status,
                XMotivo = response.Reason,
                NProt = response.Protocol,
                DhRegEvento = response.Registered,
                Id = response.Id,
                VerAplic = response.Application,
                TpAmb = response.Environment,
                COrgao = response.Orgao,
                XEvento = response.EventDescription,
                CNPJDest = response.CnpjDest,
                CPFDest = response.CpfDest,
                EmailDest = response.EmailDest,
            },
        };
    }

    private static string? Detail(ElementoEvento? detail, string name) => detail?.Filhos.FirstOrDefault(x => x.Nome == name)?.Texto;
    private sealed class Header
    {
        public ChaveAcesso? Key;
        public int? Type, Sequence, Status, Orgao, Environment;
        public string? Id, Reason, Protocol, EventVersion, Cnpj, Cpf, Application, EventDescription, CnpjDest, CpfDest, EmailDest;
        public DateTimeOffset? Date, Registered;
        public ElementoEvento? Details;
    }
    private async Task<Header> ReadHeaderAsync(CancellationToken ct)
    {
        var h = new Header { Id = reader.GetAttribute("Id") };
        await reader.ConsumeChildrenAsync(async name =>
        {
            switch (name)
            {
                case "Id":
                    h.Id = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "chNFe":
                    h.Key = ChaveAcesso.Create(await reader.ReadTextAsync().ConfigureAwait(false));
                    break;
                case "tpEvento":
                    h.Type = int.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "nSeqEvento":
                    h.Sequence = int.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "cStat":
                    h.Status = int.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "cOrgao":
                    h.Orgao = int.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "tpAmb":
                    h.Environment = int.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "dhEvento":
                    h.Date = DateTimeOffset.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "dhRegEvento":
                    h.Registered = DateTimeOffset.Parse(await reader.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    break;
                case "xMotivo":
                    h.Reason = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "nProt":
                    h.Protocol = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "verEvento":
                    h.EventVersion = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "CNPJ":
                    h.Cnpj = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "CPF":
                    h.Cpf = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "verAplic":
                    h.Application = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "xEvento":
                    h.EventDescription = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "CNPJDest":
                    h.CnpjDest = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "CPFDest":
                    h.CpfDest = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "emailDest":
                    h.EmailDest = await reader.ReadTextAsync().ConfigureAwait(false);
                    break;
                case "detEvento":
                    h.Details = await ReadDetailAsync(ct).ConfigureAwait(false);
                    break;
                default:
                    return false;
            }
            return true;
        }, options.Strict, ct).ConfigureAwait(false);
        return h;
    }
    private async Task<ElementoEvento> ReadDetailAsync(CancellationToken ct)
    {
        string name = reader.LocalName, ns = reader.NamespaceURI;
        List<AtributoEvento>? attributes = null;
        if (reader.MoveToFirstAttribute())
        {
            do
            { (attributes ??= []).Add(new(reader.LocalName, reader.NamespaceURI, reader.Value)); } while (reader.MoveToNextAttribute());
            reader.MoveToElement();
        }
        int depth = reader.Depth;
        bool empty = reader.IsEmptyElement;
        List<ElementoEvento>? children = null;
        StringBuilder? text = null;
        await reader.ReadAsync().ConfigureAwait(false);
        if (!empty)
        {
            while (!(reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth))
            {
                ct.ThrowIfCancellationRequested();
                if (reader.EOF)
                    throw new XmlException("Fim inesperado em detEvento.");
                if (reader.NodeType == XmlNodeType.Element)
                    (children ??= []).Add(await ReadDetailAsync(ct).ConfigureAwait(false));
                else
                {
                    if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.SignificantWhitespace)
                        (text ??= new()).Append(reader.Value);
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
            await reader.ReadAsync().ConfigureAwait(false);
        }
        return new(name, ns, text?.ToString(), (IReadOnlyList<AtributoEvento>?)attributes ?? [], (IReadOnlyList<ElementoEvento>?)children ?? []);
    }
}
