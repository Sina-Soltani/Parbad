import type {ReactNode} from 'react';
import Link from '@docusaurus/Link';
import Translate, {translate} from '@docusaurus/Translate';
import useBaseUrl from '@docusaurus/useBaseUrl';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import CodeBlock from '@theme/CodeBlock';

import styles from './index.module.css';

const sample = `builder.Services.AddParbad()
       .ConfigureGateways(gateways => gateways.AddMellat().WithAccounts(...))
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());

var result = await onlinePayment.RequestAsync(invoice => invoice
    .SetAmount(250_000)
    .SetCallbackUrl(callbackUrl)
    .UseAutoIncrementTrackingNumber()
    .UseMellat());`;

const features = [
  {
    title: translate({id: 'home.feature.gateways.title', message: 'Iranian gateways'}),
    text: translate({
      id: 'home.feature.gateways.text',
      message:
        'Mellat, Melli, Saman, Pasargad, Parsian, Asan Pardakht, Iran Kish, Sepehr, ZarinPal, Pay.ir, IDPay, YekPay, PayPing, Zibal and FanAva, behind one API.',
    }),
  },
  {
    title: translate({id: 'home.feature.safe.title', message: 'Safe verification'}),
    text: translate({
      id: 'home.feature.safe.text',
      message:
        'Parbad keeps the state of every payment, so a payment is verified only once and one bank payment cannot settle two orders.',
    }),
  },
  {
    title: translate({id: 'home.feature.virtual.title', message: 'Virtual Gateway'}),
    text: translate({
      id: 'home.feature.virtual.text',
      message: 'A simulated bank for development and tests, so you need no real gateway account to build your shop.',
    }),
  },
];

export default function Home(): ReactNode {
  return (
    <Layout description={translate({id: 'home.description', message: 'Online payment library for .NET'})}>
      <header className={styles.hero}>
        <div className="container">
          <img className={styles.logo} src={useBaseUrl('/img/parbad.png')} alt="Parbad" />
          <Heading as="h1" className={styles.title}>
            Parbad
          </Heading>
          <p className={styles.tagline}>
            <Translate id="home.tagline">Free and open-source online payment library for .NET</Translate>
          </p>
          <div className={styles.buttons}>
            <Link className="button button--primary button--lg" to="/docs/getting-started">
              <Translate id="home.getStarted">Get started</Translate>
            </Link>
            <Link className="button button--secondary button--lg" href="https://github.com/Sina-Soltani/Parbad">
              GitHub
            </Link>
          </div>
        </div>
      </header>
      <main className="container">
        <div className={styles.sample}>
          <CodeBlock language="csharp">{sample}</CodeBlock>
        </div>
        <div className="row">
          {features.map((feature) => (
            <div key={feature.title} className="col col--4">
              <Heading as="h3">{feature.title}</Heading>
              <p>{feature.text}</p>
            </div>
          ))}
        </div>
      </main>
    </Layout>
  );
}
