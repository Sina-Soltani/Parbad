// Creates a docs page for every gateway package from the package's README.md, so a gateway is documented in one
// place: its README, which is also shown on nuget.org. Runs before `npm start` and `npm run build`; the generated
// pages are not committed.
import {existsSync, mkdirSync, readdirSync, readFileSync, rmSync, writeFileSync} from 'node:fs';
import {dirname, join} from 'node:path';
import {fileURLToPath} from 'node:url';

const siteDir = join(dirname(fileURLToPath(import.meta.url)), '..');
const gatewaysDir = join(siteDir, '..', 'src', 'Parbad.Gateway');
const outputDir = join(siteDir, 'docs', 'gateways');
const suffix = '.generated.md';

mkdirSync(outputDir, {recursive: true});

// Remove the pages of a previous run, so a removed gateway does not leave its page behind.
for (const file of readdirSync(outputDir)) {
  if (file.endsWith(suffix)) rmSync(join(outputDir, file));
}

let count = 0;

for (const folder of readdirSync(gatewaysDir)) {
  const readmePath = join(gatewaysDir, folder, 'src', 'README.md');
  if (!existsSync(readmePath)) continue;

  const readme = readFileSync(readmePath, 'utf8').replace(/\r\n/g, '\n');
  const packageId = readme.match(/^# (.+)$/m)?.[1].trim();
  const name = readme.match(/^The (.+?) gateway for /m)?.[1];
  if (!packageId || !name) throw new Error(`${readmePath}: expected "# <package>" and "The <name> gateway for ...".`);

  const body = readme
    .replace(/^# .+\n+/, '') // The page has its own title.
    .replace(/\n## More information\n[\s\S]*$/, '\n') // Links to the repository and the docs, which this site is.
    .trim();

  const slug = folder.toLowerCase();
  const page = `---
title: ${JSON.stringify(name)}
slug: /gateways/${slug}
custom_edit_url: https://github.com/Sina-Soltani/Parbad/blob/master/src/Parbad.Gateway/${folder}/src/README.md
---

{/* Generated from src/Parbad.Gateway/${folder}/src/README.md by scripts/gateway-pages.mjs. Edit the README instead. */}

# ${name}

\`\`\`bash
dotnet add package ${packageId}
\`\`\`

${body}
`;

  writeFileSync(join(outputDir, `${slug}${suffix}`), page);
  count++;
}

console.log(`Generated ${count} gateway pages from the package READMEs.`);
