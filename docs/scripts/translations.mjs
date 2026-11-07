// Keeps the Persian pages in step with the English ones. Each translated page records in its front matter
// (source_hash) a hash of the English page it was translated from.
//
//   node scripts/translations.mjs check            Warns about translations whose English page has changed since.
//                                                  Runs before `npm start` and `npm run build`.
//   node scripts/translations.mjs stamp <file...>  Records that the given translations are up to date with their
//                                                  English page, after updating them.
import {createHash} from 'node:crypto';
import {existsSync, readdirSync, readFileSync, statSync, writeFileSync} from 'node:fs';
import {dirname, join, relative, resolve} from 'node:path';
import {fileURLToPath} from 'node:url';

const siteDir = join(dirname(fileURLToPath(import.meta.url)), '..');
const englishDir = join(siteDir, 'docs');
const translationDirs = {fa: join(siteDir, 'i18n', 'fa', 'docusaurus-plugin-content-docs', 'current')};

const read = (path) => readFileSync(path, 'utf8').replace(/\r\n/g, '\n');
const hashOf = (path) => createHash('sha256').update(read(path)).digest('hex').slice(0, 12);
const pages = (dir) =>
  readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    if (statSync(path).isDirectory()) return pages(path);
    return /\.mdx?$/.test(name) ? [path] : [];
  });
const recordedHash = (text) => text.match(/^---\n[\s\S]*?^source_hash: *(\S+)$[\s\S]*?^---$/m)?.[1];
const englishOf = (locale, translation) => join(englishDir, relative(translationDirs[locale], translation));

function check() {
  let warnings = 0;

  for (const [locale, dir] of Object.entries(translationDirs)) {
    const translated = new Set();

    for (const translation of pages(dir)) {
      const english = englishOf(locale, translation);
      const name = relative(siteDir, translation);
      translated.add(english);

      if (!existsSync(english)) {
        console.warn(`[translations] ${name}: there is no English page for it any more.`);
        warnings++;
      } else if (recordedHash(read(translation)) !== hashOf(english)) {
        console.warn(`[translations] ${name}: its English page (${relative(siteDir, english)}) has changed since it was ` +
                     `translated. Update the translation, then run: npm run translations:stamp -- ${name}`);
        warnings++;
      }
    }

    const missing = pages(englishDir).filter((english) => !translated.has(english));
    for (const english of missing) {
      console.info(`[translations] ${locale}: ${relative(siteDir, english)} is not translated; the English page is shown.`);
    }
  }

  console.log(warnings === 0 ? '[translations] All translations are up to date.' : `[translations] ${warnings} translation(s) to update.`);
}

function stamp(files) {
  if (files.length === 0) throw new Error('Name the translations to stamp.');

  for (const file of files) {
    const translation = resolve(file);
    const locale = Object.keys(translationDirs).find((l) => translation.startsWith(translationDirs[l]));
    if (!locale) throw new Error(`${file} is not a translated docs page.`);

    const raw = readFileSync(translation, 'utf8');
    const newline = raw.includes('\r\n') ? '\r\n' : '\n';
    const text = raw.replace(/\r\n/g, '\n');
    const hash = hashOf(englishOf(locale, translation));
    const front = text.match(/^---\n([\s\S]*?)\n---\n/);
    if (!front) throw new Error(`${file} has no front matter.`);

    const fields = front[1].split('\n').filter((line) => !line.startsWith('source_hash:'));
    fields.push(`source_hash: ${hash}`);
    const updated = `---\n${fields.join('\n')}\n---\n${text.slice(front[0].length)}`;
    writeFileSync(translation, updated.replace(/\n/g, newline));
    console.log(`[translations] ${relative(siteDir, translation)}: up to date with its English page (${hash}).`);
  }
}

const [command, ...args] = process.argv.slice(2);
if (command === 'stamp') stamp(args);
else check();
