import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { Providers } from './providers'
import { fonts } from './fonts'
import { Nav } from './nav'

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Make Movies",
  description: "Family proof movie library management",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
    <head>
      <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png?v=1"/>
      <link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png?v=1"/>
      <link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png?v=1"/>
      <link rel="manifest" href="/site.webmanifest?v=1"/>
      <link rel="mask-icon" href="/safari-pinned-tab.svg?v=1" color="#5bbad5"/>
      <link rel="shortcut icon" href="/favicon.ico?v=1"/>
      <meta name="apple-mobile-web-app-title" content="Make Movies"/>
      <meta name="application-name" content="Make Movies"/>
      <meta name="msapplication-TileColor" content="#da532c"/>
      <meta name="theme-color" content="#ffffff"/>
    </head>
    <body className={fonts.rubik.variable}>
    <Providers>
      <Nav></Nav>
      <main>
        {children}
      </main>
    </Providers>
    </body>
    </html>
  );
}
