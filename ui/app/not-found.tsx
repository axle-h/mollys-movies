'use client';

import {Alert, AlertDescription, AlertIcon, AlertTitle, Container} from '@chakra-ui/react'
import {Link} from "@chakra-ui/next-js";

export default function NotFoundPage() {
    return  (
        <Container py={4}>
            <Alert status='error'>
                <AlertIcon />
                <AlertTitle>Page not found</AlertTitle>
                <AlertDescription>
                    Go back <Link href="/">home</Link>
                </AlertDescription>
            </Alert>
        </Container>

    )
}