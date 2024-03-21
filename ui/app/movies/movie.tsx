import {Movie} from "@/client/models";
import {
    Badge,
    Box,
    CardBody,
    Flex,
    Heading,
    Image,
    Stack,
    Text,
    Tooltip
} from "@chakra-ui/react";
import {CheckCircleIcon, StarIcon} from "@chakra-ui/icons";
import {ResponsiveValue} from "@chakra-ui/styled-system";
import {Property} from "csstype";

export function MovieImage({ movie, maxW }: { movie: Movie, maxW: string | number | ResponsiveValue<string | number> }) {
    return (<Image
        maxW={maxW}
        src={`${process.env.NEXT_PUBLIC_API_BASE_URL}/api/v1/movie/${movie.id}/image`}
        alt={movie.title ?? ''}
        style={{ objectFit: 'cover' }}
    />)
}

export function MovieCardBody({ movie, descriptionLines, displayDescription }: { movie: Movie, descriptionLines?: ResponsiveValue<number>, displayDescription?: ResponsiveValue<Property.Display> }) {
    return (<Stack>
        <CardBody>
            <Heading size='md'>
                {movie.inLibrary ? <Tooltip label='Already downloaded'><CheckCircleIcon color='green' mr={2} /></Tooltip> : <></>}

                <Text mr={2} style={{display: 'inline'}}>
                    {movie.title}
                </Text>

                <Badge colorScheme='purple'>{movie.year}</Badge>
            </Heading>

            <Text py={2}>
                {movie.genres
                    ?.toSorted((g1, g2) => g1.localeCompare(g2))
                    ?.map(g => <Badge mr='1' key={g} colorScheme='blue'>{g}</Badge>)}
            </Text>

            <Flex>
                <Box as={StarIcon} color="orange.400" />
                <Text ml={1} fontSize="sm">
                    <b>{movie.rating}</b>
                </Text>
            </Flex>

            <Flex display={displayDescription}>
                <Text my={2} noOfLines={descriptionLines}>
                    {movie.description}
                </Text>
            </Flex>
        </CardBody>
    </Stack>)
}