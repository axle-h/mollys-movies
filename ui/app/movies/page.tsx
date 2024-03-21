'use client';

import {
    Card,
    Badge,
    Container,
    CardBody,
    Text,
    Stack,
    Heading,
    Input,
    Image,
    Flex,
    Box,
    InputGroup,
    InputLeftElement,
    LinkBox,
    LinkOverlay
} from '@chakra-ui/react'
import {SearchIcon, StarIcon} from '@chakra-ui/icons'
import { useClient } from '@/client'
import {Link} from "@chakra-ui/next-js";
import {useEffect, useState} from "react";
import useDebounce from "@/app/debounce";
import { useRouter } from 'next/navigation'
import {Pagination} from "@/app/pagination";
import {Error, Loading, NoData} from "@/app/alert";
import {MovieCardBody, MovieImage} from "@/app/movies/movie";

function MovieList({ searchTerm, page, updatePage }: { searchTerm: string, page: number, updatePage: (page: number) => void }) {
    const limit = 10;
    const [pageCount, updatePageCount] = useState<number | null>(null);
    const { data: movies, error, isLoading } = useClient({
        api: 'list-movies',
        page,
        limit,
        search: searchTerm
    })

    useEffect(() => {
        if (movies?.count) {
            updatePageCount(Math.ceil(movies.count / limit))
        }
    }, [movies?.count, limit]);

    if (isLoading) {
        return <Loading />
    }

    if (error) {
        return <Error error={error} />
    }

    if (!movies?.data?.length) {
        return <NoData />
    }

    const cards = movies.data
        .map(movie => (
            <LinkBox as={Card}
                key={movie.id}
                direction='row'
                overflow='hidden'
                variant='outline'
                height={{ base: 180, sm: 230 }}
                my={3}
            >
                <LinkOverlay as={Link} href={`/movies/${movie.id}`} />
                <MovieImage movie={movie} maxW={200} />
                <MovieCardBody movie={movie} descriptionLines={{ sm: 2, md: 3 }} displayDescription={{ base: 'none', sm: 'block' }} />
            </LinkBox>
        ))
    return (<>
        {cards}
        {pageCount ?
            <Pagination current={page} count={pageCount} onPaginate={updatePage} />
            : <></>}
    </>)
}

export default function MoviesHome({ searchParams }: { searchParams: { search?: string, page?: string } }) {
    const router = useRouter()
    const [searchTerm, setSearchTerm] = useState(searchParams.search ?? '')
    const currentPage = Number(searchParams?.page) || 1;

    function navigate({ nextPage, nextSearchTerm }: { nextPage?: number, nextSearchTerm?: string }) {
        router.replace(`?search=${nextSearchTerm || searchTerm}&page=${nextPage || currentPage}`)
    }

    const handleSearch = useDebounce(
        (nextSearchTerm: string) => navigate({ nextSearchTerm, nextPage: 1 }),
        500);

    return (<Container py={4}>
        <Heading mb={4}>Movies</Heading>
        <InputGroup size='lg' mb={4}>
            <InputLeftElement pointerEvents='none'>
                <SearchIcon color='gray.300' />
            </InputLeftElement>
            <Input placeholder='Search movies'
                   value={searchTerm}
                   onChange={(event) => {
                       const { value } = event.target
                       setSearchTerm(value)
                       // Debounce the search callback
                       handleSearch(value)
                   }}/>
        </InputGroup>

        <MovieList
            searchTerm={searchParams.search ?? ''}
            page={currentPage}
            updatePage={(nextPage) => navigate({nextPage})} />
    </Container>);
}