'use client';

import {
    Button,
    ButtonGroup,
    Container,
    Heading,
    Spinner,
    Table,
    Text,
    Thead,
    Tbody,
    Tr,
    Th,
    Td,
    TableContainer,
    useToast,
} from '@chakra-ui/react'
import {useEffect, useState} from "react";
import {apiClient, useClient} from '@/client'
import {ScrapePaginatedData} from "@/client/models";
import {Pagination} from "@/app/pagination";
import {BoolIcon} from "@/app/icons";
import {AddIcon, RepeatIcon} from "@chakra-ui/icons";
import {NoData, Error, Loading} from "@/app/alert";

interface ListPagination {
    page: number,
    limit: number
}

function ScrapeControls({ onRefresh, onNew }: { onRefresh: (() => Promise<any>), onNew: (() => Promise<any>) }) {
    return (
        <ButtonGroup variant='outline' mb={4}>
            <Button colorScheme='blue' leftIcon={<AddIcon />} onClick={onNew}>New</Button>
            <Button leftIcon={<RepeatIcon />} onClick={onRefresh}>Refresh</Button>
        </ButtonGroup>
    )
}

const dateFormatter = new Intl.DateTimeFormat('en-US',
    { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
const numberFormatter = new Intl.NumberFormat('en-US');

function ScrapeList({ scrapes }: { scrapes?: ScrapePaginatedData }) {
    if (!scrapes?.data?.length) {
        return <NoData />
    }

    const rows = scrapes.data.map(s =>
        (<Tr key={s.id}>
            <Td>{dateFormatter.format(s.startDate ?? new Date())}</Td>
            <Td>{ s.success !== undefined
                ? <BoolIcon value={s.success} />
                : <Spinner /> }</Td>
            <Td>{numberFormatter.format(s.movieCount ?? 0)}</Td>
            <Td>{numberFormatter.format(s.torrentCount ?? 0)}</Td>
        </Tr>))

    return (<>
        <TableContainer>
            <Table variant='simple'>
                <Thead>
                    <Tr>
                        <Th>Start date</Th>
                        <Th>Status</Th>
                        <Th>Movies</Th>
                        <Th>Torrents</Th>
                    </Tr>
                </Thead>
                <Tbody>
                    {rows}
                </Tbody>
            </Table>
        </TableContainer>
    </>);
}

export default function ScraperHome() {
    const [pageCount, updatePageCount] = useState<number | null>(null);
    const [pagination, updatePagination] = useState<ListPagination>({ page: 1, limit: 10 });
    const { data: scrapes, error, isLoading, mutate } = useClient({
        api: 'list-scrapes',
        ...pagination
    })
    const toast = useToast()

    useEffect(() => {
        if (scrapes?.count) {
            updatePageCount(Math.ceil(scrapes.count / pagination.limit))
        }
    }, [scrapes?.count, pagination.limit]);


    async function refresh() {
        if (pagination.page > 1) {
            updatePagination({ ...pagination, page: 1 })
        } else {
            await mutate()
        }
    }

    async function newScrape() {
        try {
            await apiClient.api.v1.scrape.post()
            await mutate()
            toast({
                title: 'Success',
                description: "New scrape started.",
                status: 'success',
                duration: 5000,
                isClosable: true,
            })
        } catch (e) {
            console.error(e);
            toast({
                title: 'Fail',
                description: "Failed to create new scrape.",
                status: 'error',
                duration: 5000,
                isClosable: true,
            })
        }
    }

    return (<Container py={4}>
        <Heading mb={4}>Scraper</Heading>
        <ScrapeControls
            onRefresh={refresh}
            onNew={newScrape}
        />
        {
            isLoading ? <Loading />
                : error ? <Error error={error} />
                : <ScrapeList scrapes={scrapes} />
        }
        {
            pageCount ?
                <Pagination current={pagination.page} count={pageCount} onPaginate={(page) => updatePagination({ ...pagination, page })} />
                : <></>
        }

    </Container>);
}