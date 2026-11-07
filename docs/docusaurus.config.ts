import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Parbad',
  tagline: 'Online payment library for .NET',
  favicon: 'img/parbad.png',

  future: {
    v4: true,
  },

  // Published with GitHub Pages at https://sina-soltani.github.io/Parbad/
  url: 'https://sina-soltani.github.io',
  baseUrl: '/Parbad/',
  organizationName: 'Sina-Soltani',
  projectName: 'Parbad',
  trailingSlash: false,

  onBrokenLinks: 'throw',

  markdown: {
    // `## Heading {#anchor}`: the translated pages keep the English anchors, so links work in every language.
    mdx1Compat: {headingIds: true},
  },

  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'fa'],
    localeConfigs: {
      en: {label: 'English'},
      fa: {label: 'فارسی', direction: 'rtl', htmlLang: 'fa-IR'},
    },
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/Sina-Soltani/Parbad/tree/master/docs/',
          lastVersion: 'current',
          versions: {
            current: {label: '4.0'},
            '3.x': {label: '3.x', banner: 'none'},
          },
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  clientModules: ['./src/fonts.ts'],

  themes: [
    [
      '@easyops-cn/docusaurus-search-local',
      {
        hashed: true,
        language: ['en'],
        docsRouteBasePath: '/docs',
        indexBlog: false,
      },
    ],
  ],

  themeConfig: {
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Parbad',
      logo: {
        alt: 'Parbad',
        src: 'img/parbad.png',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docs',
          position: 'left',
          label: 'Docs',
        },
        {
          type: 'docsVersionDropdown',
          position: 'right',
        },
        {
          type: 'localeDropdown',
          position: 'right',
        },
        {
          href: 'https://github.com/Sina-Soltani/Parbad',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Introduction', to: '/docs/intro'},
            {label: 'Getting started', to: '/docs/getting-started'},
          ],
        },
        {
          title: 'Community',
          items: [
            {label: 'Discussions', href: 'https://github.com/Sina-Soltani/Parbad/discussions'},
            {label: 'Issues', href: 'https://github.com/Sina-Soltani/Parbad/issues'},
          ],
        },
        {
          title: 'More',
          items: [
            {label: 'GitHub', href: 'https://github.com/Sina-Soltani/Parbad'},
            {label: 'NuGet', href: 'https://www.nuget.org/packages/Parbad/'},
            {label: 'Samples', href: 'https://github.com/Sina-Soltani/Parbad.Samples'},
          ],
        },
      ],
      copyright: `Copyright © 2016-${new Date().getFullYear()} Sina Soltani`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'bash', 'powershell'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
