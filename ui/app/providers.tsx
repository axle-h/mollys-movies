'use client'

import {ChakraProvider, defineStyleConfig, extendBaseTheme} from '@chakra-ui/react'

const {
  Badge,
  Button,
  Card,
  Menu,
  Alert,
  Container,
  Spinner,
  Heading,
  Table,
  Input,
  Form,
  Tooltip,
  Progress
} = chakraTheme.components

import {theme as chakraTheme} from "@chakra-ui/theme";

const theme = extendBaseTheme({
  initialColorMode: 'dark',
  useSystemColorMode: false,
  fonts: {
    heading: 'var(--font-rubik)',
    body: 'var(--font-rubik)',
  },
  components: {
    Badge,
    Button,
    Card,
    Menu,
    Alert,
    Container: defineStyleConfig({
      ...Container,
      baseStyle: {
        w: '100%',
        mx: 'auto',
        maxW: '100ch',
        px: '4'
      }
    }),
    Spinner,
    Heading,
    Table,
    Input,
    Form,
    Tooltip,
    Progress
  },
});

export function Providers({ children }: { children: React.ReactNode }) {
  return <ChakraProvider theme={theme}>{children}</ChakraProvider>
}