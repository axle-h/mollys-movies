'use client';

import {
    Badge,
    Button,
    ButtonGroup, Card, CardBody,
    Container,
    Heading,
    Progress,
    Stack,
    Text
} from "@chakra-ui/react";
import {useState} from "react";
import {useClient} from "@/client";
import {CheckIcon, RepeatIcon} from "@chakra-ui/icons";
import {Error, Loading, NoData} from "@/app/alert";
import {Pagination} from "@/app/pagination";
import {Download, DownloadPaginatedData} from "@/client/models";
import { MovieImage} from "@/app/movies/movie";

interface ListPagination {
    page: number,
    limit: number
}

function DownloadControls({ onRefresh }: { onRefresh: (() => Promise<any>) }) {
    return (
        <ButtonGroup variant='outline' mb={4}>
            <Button leftIcon={<RepeatIcon />} onClick={onRefresh}>Refresh</Button>
        </ButtonGroup>
    )
}

function DownloadStatus({ download }: { download: Download }) {
    const percentDone = download.stats?.percentDone
        ? Math.round((download.stats.percentDone * 100 + Number.EPSILON) * 100) / 100
        : null;

    const progressBar = download.complete === true
        ? <Progress size='sm' value={100} colorScheme='green' />
        : percentDone
            ? <Progress value={percentDone} size='sm' />
            : <Progress size='sm' isIndeterminate />

    return (
        <Stack>
            {progressBar}
            <Text fontSize='xs' as='i'>
                { download.complete ? 'Downloaded' : percentDone ? `${percentDone}%` : '' }
                <Eta value={download.stats?.eta} />
            </Text>
        </Stack>
    )
}

function Eta({ value }: { value: string | undefined }) {
    if (!value) return <></>
    const match = value.match(/(\d{2}):(\d{2}):(\d{2})/)
    if (!match) {
        return <></>
    }

    const str = ['hr', 'min', 'sec'].map((name, index) => {
        const componentValue = parseInt(match[index + 1])
        if (componentValue === 1) {
            return `1 ${name}`
        } else if (componentValue > 0) {
            return `${componentValue} ${name}s`
        }
    }).join(' ')

    return str.length > 0 ? <> - eta {str}</> : <></>;
}

function DownloadList({ downloads }: { downloads?: DownloadPaginatedData }) {
    if (!downloads?.data?.length) {
        return <NoData />
    }

    const cards = downloads.data
        .map(download => {
            if (!download.name) {
                return <></>
            }
            const match = download.name.match(/^(.+) \((\d{4})\)$/)
            const title = match ? match[1] : download.name;
            const year = match ? parseInt(match[2]) : null
            return (
                <Card key={download.id}
                      direction='row'
                      overflow='hidden'
                      variant='outline'
                      height={{ base: 180, sm: 230 }}
                      my={3}
                >
                    <MovieImage movie={{ id: download.movieId, title }} maxW={200} />
                    <CardBody>
                        <Heading size='md' mb={4}>
                            <Text mr={2} style={{display: 'inline'}}>
                                {title}
                            </Text>
                            { year ? <Badge colorScheme='purple'>{year}</Badge> : <></> }
                        </Heading>
                        <DownloadStatus download={download} />
                    </CardBody>
                </Card>
            )
        })
    return <>{cards}</>
}

export default function DownloadsHome() {
    const [pageCount, updatePageCount] = useState<number | null>(null);
    const [pagination, updatePagination] = useState<ListPagination>({ page: 1, limit: 10 });
    const { data: downloads, error, isLoading, mutate } = useClient({
        api: 'list-downloads',
        ...pagination
    })

    async function refresh() {
        if (pagination.page > 1) {
            updatePagination({ ...pagination, page: 1 })
        } else {
            await mutate()
        }
    }

    return (<Container py={4}>
        <Heading mb={4}>Downloads</Heading>
        <DownloadControls onRefresh={refresh} />

        {
            isLoading ? <Loading />
                : error ? <Error error={error} />
                    : <DownloadList downloads={downloads} />
        }
        {
            pageCount ?
                <Pagination current={pagination.page} count={pageCount} onPaginate={(page) => updatePagination({ ...pagination, page })} />
                : <></>
        }
    </Container>)
}