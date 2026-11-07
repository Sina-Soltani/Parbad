# Parbad documentation

The documentation site of Parbad, built with [Docusaurus](https://docusaurus.io/) and published at
https://sina-soltani.github.io/Parbad/.

## Running it locally

Requires Node.js 20 or later.

```bash
cd docs
npm install
npm run preview           # The whole site, both languages: http://localhost:3000/Parbad/
```

While writing, `npm start` reloads a page as soon as it is saved. It serves one language at a time, so the language
switcher leads to a "page not found" there:

```bash
npm start                 # English: http://localhost:3000/Parbad/
npm start -- --locale fa  # Persian: http://localhost:3000/Parbad/fa/
```

## Structure

| Folder | Content |
|---|---|
| `docs/` | The pages of the current version (4.0) |
| `docs/gateways/` | The gateways built into the `Parbad` package. The pages of the gateway packages are generated from their `README.md` (`src/Parbad.Gateway/*/src/README.md`) by `scripts/gateway-pages.mjs` before every start and build; edit the README, not the generated page |
| `versioned_docs/version-3.x/` | The pages of 3.x, frozen |
| `i18n/fa/` | The Persian translations; a page without a translation is shown in English |
| `src/pages/` | The home page |
| `static/img/` | Images |

## Translations

Each Persian page records, in its front matter (`source_hash`), the English page it was translated from. Before every
start and build, `scripts/translations.mjs` warns about each Persian page whose English page has changed since, for
example after a gateway's README was edited. Update the Persian page, then record that it is up to date:

```bash
npm run translations:stamp -- i18n/fa/docusaurus-plugin-content-docs/current/gateways/zibal.generated.md
```

`npm run translations:check` runs the check on its own.

## Versions

Before the pages are changed for the next major version, `npm run docusaurus docs:version 4.0` freezes the current
pages as version 4.0.
