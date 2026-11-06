# Parbad documentation

The documentation site of Parbad, built with [Docusaurus](https://docusaurus.io/) and published at
https://sina-soltani.github.io/Parbad/.

## Running it locally

Requires Node.js 20 or later.

```bash
cd docs
npm install
npm start                 # English, with live reload: http://localhost:3000/Parbad/
npm start -- --locale fa  # Persian
```

`npm start` serves one language at a time. To see both languages and the language switcher together:

```bash
npm run build
npm run serve             # http://localhost:3000/Parbad/
```

## Structure

| Folder | Content |
|---|---|
| `docs/` | The pages of the current version (4.0) |
| `versioned_docs/version-3.x/` | The pages of 3.x, frozen |
| `i18n/fa/` | The Persian translations; a page without a translation is shown in English |
| `src/pages/` | The home page |
| `static/img/` | Images |

Before the pages are changed for the next major version, `npm run docusaurus docs:version 4.0` freezes the current
pages as version 4.0.
