'use client'

import { useEffect } from 'react'
import {Alert, AlertDescription, AlertIcon, AlertTitle, Button, Container} from "@chakra-ui/react";
import {Link} from "@chakra-ui/next-js";

export default function Error({ error, reset }: {
    error: Error & { digest?: string }
    reset: () => void
}) {
    return (
        <Container py={4}>
            <Alert status='error'>
                <AlertIcon />
                <AlertTitle>Something went wrong</AlertTitle>
                <AlertDescription>
                    Go back <Link href="/">home</Link>
                </AlertDescription>
            </Alert>
        </Container>
    )
}