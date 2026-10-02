"""Offline XSD inventory. No runtime dependency or network resolution."""
import argparse
import json
from pathlib import Path
import xml.etree.ElementTree as ET

XS = '{http://www.w3.org/2001/XMLSchema}'


class Schema:
    def __init__(self, directory):
        self.types = {}
        self.elements = {}
        self.groups = {}
        self.attributes = {}
        for path in sorted(Path(directory).glob('*.xsd')):
            root = ET.parse(path).getroot()
            for child in root:
                name = child.get('name')
                if name:
                    target = (self.types if child.tag in (XS+'complexType', XS+'simpleType') else
                              self.elements if child.tag == XS+'element' else
                              self.attributes if child.tag == XS+'attribute' else self.groups)
                    target[name] = child

    def resolve(self, node):
        ref = node.get('ref')
        if ref:
            return (self.attributes if node.tag == XS+'attribute' else self.elements)[ref.split(':')[-1]]
        return node

    def type_node(self, node):
        node = self.resolve(node)
        for kind in ('complexType', 'simpleType'):
            inline = node.find(XS+kind)
            if inline is not None:
                return inline
        return self.types.get(node.get('type', '').split(':')[-1])

    def base(self, node, seen=()):
        node = self.resolve(node)
        typ = node.get('type', '').split(':')[-1]
        if typ == 'TData': return 'date'
        if typ == 'TDateTimeUTC': return 'dateTime'
        if typ in seen:
            raise ValueError('Cycle in simple type '+typ)
        tn = self.type_node(node)
        if tn is None:
            return typ or 'string'
        restriction = tn.find(XS+'restriction')
        if restriction is None:
            return 'complex' if tn.tag == XS+'complexType' else 'string'
        base = restriction.get('base', '').split(':')[-1]
        if base in self.types:
            return self.base(ET.Element(XS+'element', type=base), (*seen, typ))
        # Decimal lexical restrictions in NF-e derive from string.
        pattern = restriction.find(XS+'pattern')
        if base == 'string' and typ.startswith(('TDec', 'Tporcent')):
            return 'decimal'
        return base

    def children(self, type_node):
        def walk(node, optional=False, repeated=False, choice=False):
            for child in node:
                kind = child.tag.removeprefix(XS)
                if kind in ('annotation', 'simpleType', 'complexType'):
                    continue
                opt = optional or child.get('minOccurs') == '0'
                many = repeated or child.get('maxOccurs', '1') != '1'
                if kind in ('element', 'attribute'):
                    actual = self.resolve(child)
                    yield {'node': actual, 'name': actual.get('name'), 'attribute': kind == 'attribute',
                           'min': 0 if opt or choice or kind == 'attribute' and child.get('use') != 'required' else 1,
                           'max': ('unbounded' if repeated else child.get('maxOccurs', '1')) if many else 1,
                           'choice': choice}
                elif kind in ('extension', 'restriction'):
                    base = child.get('base', '').split(':')[-1]
                    if base in self.types:
                        yield from walk(self.types[base], opt, many, choice)
                    yield from walk(child, opt, many, choice)
                elif kind in ('group', 'attributeGroup') and child.get('ref'):
                    yield from walk(self.groups[child.get('ref').split(':')[-1]], opt, many, choice)
                elif kind in ('sequence', 'choice', 'all', 'complexContent', 'simpleContent'):
                    yield from walk(child, opt, many, choice or kind == 'choice')
        return list(walk(type_node))

    def inventory(self, root_type='TNFe'):
        rows = []
        def visit(node, path, ancestry):
            ident = id(node)
            if ident in ancestry:
                raise ValueError('Cycle at '+path)
            for field in self.children(node):
                child = field.pop('node')
                if path == root_type and field['name'] == 'Signature':
                    continue
                p = path+'/'+('@' if field['attribute'] else '')+field['name']
                maximum = field['max']
                field['max'] = int(maximum) if str(maximum).isdigit() else maximum
                field.update(path=p, type=child.get('type'), base=self.base(child))
                rows.append(field)
                tn = self.type_node(child)
                if tn is not None and tn.tag == XS+'complexType':
                    visit(tn, p, (*ancestry, ident))
        visit(self.types[root_type], root_type, ())
        return rows


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--schema-dir', required=True)
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    Path(args.output).write_text(json.dumps(Schema(args.schema_dir).inventory(), indent=2, ensure_ascii=False)+'\n', encoding='utf-8')
