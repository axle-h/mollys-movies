'use client';

import {
    Button,
    ButtonGroup,
    Card,
    Center,
    Container,
    Tooltip, useToast
} from "@chakra-ui/react";
import {ArrowBackIcon, DownloadIcon, LockIcon} from "@chakra-ui/icons";
import { useRouter } from 'next/navigation'
import {apiClient, useClient} from '@/client'
import {Error, Loading, NotFound} from "@/app/alert";
import {Movie} from "@/client/models";
import {MovieCardBody, MovieImage} from "@/app/movies/movie";

function MovieCard({ movie }: { movie: Movie }) {
    return (
        <Card direction={{ base: 'column', md: 'row' }}
              overflow='auto'
              variant='filled'
              mb={4}
              bg='transparent'
        >
            <Center>
                <MovieImage movie={movie} maxW={{ base: 200, sm: 300 }} />
            </Center>

            <MovieCardBody movie={movie} />
        </Card>
    )
}

export default function MoviePage({ params: { id } }: { params: { id: string } }) {
    const router = useRouter()
    const { data: movie, error, isLoading, mutate } = useClient({ api: 'get-movie', id })
    const toast = useToast()

    async function download() {
        try {
            await apiClient.api.v1.movie.byId(id).download.post();
            await mutate();
            toast({
                title: 'Success',
                description: `Downloading ${movie?.title}.`,
                status: 'success',
                duration: 5000,
                isClosable: true,
            })
        } catch (e) {
            console.error(e);
            toast({
                title: 'Fail',
                description: `Failed to download ${movie?.title}.`,
                status: 'error',
                duration: 5000,
                isClosable: true,
            })
        }
    }

    const inLibrary = movie?.inLibrary === true
    return (<Container py={4}>
        <ButtonGroup variant='outline' mb={4}>
            <Button leftIcon={<ArrowBackIcon />} variant='outline' onClick={router.back} mb={4}>
                Back
            </Button>
            <Tooltip isDisabled={!inLibrary} label='Already downloaded'>
                <Button leftIcon={inLibrary ? <LockIcon /> : <DownloadIcon />}
                        colorScheme='green'
                        isLoading={!movie}
                        isDisabled={inLibrary}
                        onClick={download}>Download</Button>
            </Tooltip>

        </ButtonGroup>

        {isLoading ? <Loading /> :
            error ? <Error error={error} /> :
                !movie ? <NotFound entity='movie' id={id} /> :
                    <MovieCard movie={movie} />}
    </Container>)
}