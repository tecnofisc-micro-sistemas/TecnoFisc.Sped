"""Content-addressed storage for converted manual pages."""

from __future__ import annotations

from dataclasses import dataclass
from hashlib import sha256
from pathlib import Path


@dataclass(frozen=True)
class CacheKey:
    pdf_sha256: str
    converter_sha256: str
    page: int

    def output_path(self, cache_dir: Path) -> Path:
        return cache_dir / self.pdf_sha256 / self.converter_sha256 / f"page-{self.page:04d}.md"


def sha256_file(path: Path) -> str:
    return sha256(path.read_bytes()).hexdigest()


def converter_fingerprint(package_dir: Path | None = None) -> str:
    """Hash every current package source deterministically, including future fixups."""
    source_dir = package_dir or Path(__file__).parent
    digest = sha256()
    for source in sorted(source_dir.rglob("*.py"), key=lambda path: path.relative_to(source_dir).as_posix()):
        relative_path = source.relative_to(source_dir).as_posix().encode("utf-8")
        source_bytes = source.read_bytes()
        digest.update(len(relative_path).to_bytes(4, "big"))
        digest.update(relative_path)
        digest.update(len(source_bytes).to_bytes(8, "big"))
        digest.update(source_bytes)
    return digest.hexdigest()
