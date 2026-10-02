import tempfile
import unittest
import json
from pathlib import Path
from inventory import Schema


class InventoryTests(unittest.TestCase):
    def test_committed_inventory_matches_pinned_schema(self):
        root = Path(__file__).resolve().parents[2]
        schema = Schema(root/'sped/nfe/schemas')
        expected = schema.inventory() + schema.inventory('TProtNFe')
        self.assertEqual(expected, json.loads((root/'sped/nfe/inventory.json').read_text(encoding='utf-8')))

    def schema(self, body):
        tmp = tempfile.TemporaryDirectory()
        self.addCleanup(tmp.cleanup)
        path = Path(tmp.name)
        (path / 'test.xsd').write_text('<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">' + body + '</xs:schema>')
        return Schema(path)

    def test_shared_types_choices_repetition_attributes(self):
        schema = self.schema('''
        <xs:complexType name="Shared"><xs:sequence><xs:element name="value" type="xs:decimal"/></xs:sequence><xs:attribute name="id" type="xs:string"/></xs:complexType>
        <xs:complexType name="Root"><xs:sequence><xs:element name="first" type="Shared"/><xs:choice minOccurs="0"><xs:element name="second" type="Shared" maxOccurs="10"/><xs:element name="other" type="xs:string"/></xs:choice></xs:sequence></xs:complexType>''')
        rows = {x['path']: x for x in schema.inventory('Root')}
        self.assertIn('Root/first/value', rows)
        self.assertIn('Root/second/@id', rows)
        self.assertEqual(0, rows['Root/second']['min'])
        self.assertEqual(10, rows['Root/second']['max'])
        self.assertTrue(rows['Root/second']['choice'])
        self.assertEqual('decimal', rows['Root/first/value']['base'])

    def test_cycle_is_rejected(self):
        schema = self.schema('<xs:complexType name="Loop"><xs:sequence><xs:element name="self" type="Loop"/></xs:sequence></xs:complexType>')
        with self.assertRaisesRegex(ValueError, 'Cycle'):
            schema.inventory('Loop')

    def test_repeated_sequence_preserves_finite_bound(self):
        schema = self.schema('<xs:complexType name="Root"><xs:sequence maxOccurs="5"><xs:element name="value" type="xs:string" maxOccurs="2"/></xs:sequence></xs:complexType>')
        self.assertEqual(10, schema.inventory('Root')[0]['max'])

    def test_new_shared_leaf_changes_both_paths(self):
        schema = self.schema('<xs:complexType name="Shared"><xs:sequence><xs:element name="new" type="xs:string"/></xs:sequence></xs:complexType><xs:complexType name="Root"><xs:sequence><xs:element name="a" type="Shared"/><xs:element name="b" type="Shared"/></xs:sequence></xs:complexType>')
        paths = {x['path'] for x in schema.inventory('Root')}
        self.assertTrue({'Root/a/new', 'Root/b/new'}.issubset(paths))

    def test_references_and_inherited_attributes(self):
        schema = self.schema('''<xs:element name="entry" type="xs:string"/>
        <xs:complexType name="Base"><xs:attribute name="id" type="xs:string" use="required"/></xs:complexType>
        <xs:complexType name="Root"><xs:complexContent><xs:extension base="Base"><xs:sequence><xs:element ref="entry" maxOccurs="3"/></xs:sequence></xs:extension></xs:complexContent></xs:complexType>''')
        rows={r['path']:r for r in schema.inventory('Root')}
        self.assertEqual(1, rows['Root/@id']['min'])
        self.assertEqual(3, rows['Root/entry']['max'])

    def test_version_and_qrcode_are_not_decimals(self):
        schema=self.schema('''<xs:simpleType name="TVersion"><xs:restriction base="xs:string"><xs:pattern value="4\\.00"/></xs:restriction></xs:simpleType>
        <xs:complexType name="Root"><xs:sequence><xs:element name="version" type="TVersion"/></xs:sequence></xs:complexType>''')
        self.assertEqual('string', schema.inventory('Root')[0]['base'])
