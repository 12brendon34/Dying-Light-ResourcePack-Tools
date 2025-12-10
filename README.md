# Dying Light Resource Pack Extractor

This tool extracts and processes game resource files from the *Dying Light* series.

It unpacks `.rpack` resource packs and exports the supported file formats.

---

## Supported games & formats

| Game                                        | Textures | Mesh | ANM2 | Raw binary |
| ------------------------------------------- | -------- | ---- | ---- | ---------- |
| **Dying Light: The Beast**                  | ✓        | P    | ✓    | ✓          |
| **Dying Light 2**                           | ✓        | -    | ✓    | ✓          |
| **Dying Light: Bad Blood**                  | ✓        | -    | ✓    | ✓          |
| **Dying Light**                             | ✓        | -    | ✓    | ✓          |
| **Dead Island: Riptide Definitive Edition** | ✓        | -    | ✓    | ✓          |
| **Dead Island: Definitive Edition**         | ✓        | -    | ✓    | ✓          |
| **FIM Speedway Grand Prix 15**              | ✓        | -    | ✓    | ✓          |


Notes:

* **Textures** — `.DDS` files are exported directly; `.PNG.DDS` files are converted back to `.PNG`.
* **Mesh** — `.MSH` mesh files are exported and converted to the Chrome Engine editor mesh format. `(Experimental)`
* **ANM2** — `.ANM2` files are exported as raw animation data.
* **Other/Unknown** — Any other unsupported format is dumped as a raw binary.

---

## Usage (Linux example)

Run the unpacker on an `.rpack` file:

```bash
./RP6_UnpackCLI engine_PC.rpack
```

On first run the tool will create (or you can create manually) an `options.ini` file. When present, the extractor will read this file to apply runtime fixups.

Example `options.ini`:

```ini
; RP6_UnpackCLI options
[Fixups]
EnablePngFixup=true
EnableRawDumping=true
```

### What the options do

* `EnablePngFixup` — Reverses a common `Chrome Engine Resource Pack Compiler` step: the compiler converts PNGs to DDS and renames them (e.g. `texture.png.dds`). When this option is enabled the extractor will detect `.png.dds` files and convert them back to `.png` on export.

* `EnableRawDumping` — When enabled, all fixups are disabled and every chunk is dumped as a raw binary file.
