import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docs: [
    'intro',
    'getting-started',
    'configuration',
    'storage',
    'virtual-gateway',
    {
      type: 'category',
      label: 'Gateways',
      items: ['gateways/zarinpal'],
    },
    'migrating-to-4',
  ],
};

export default sidebars;
