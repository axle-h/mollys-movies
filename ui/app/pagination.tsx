'use client';

import {
    Flex,
    Button,
    Icon
} from '@chakra-ui/react'
import {ArrowBackIcon, ArrowForwardIcon} from "@chakra-ui/icons";

export function Pagination({ current, count, onPaginate }: { current: number, count: number, onPaginate: ((page: number) => void) }) {
    const PagButton = (props: { page: number, active?: boolean, disabled?: boolean, children: React.ReactNode }) => {
        const activeStyle = {
            bg: "blue.600",
            color: "white",
            _dark: {
                color: "white",
                bg: "blue.500",
            },
        };
        return (
            <Button
                mx={1}
                px={4}
                py={2}
                rounded="md"
                bg="transparent"
                _dark={{
                    color: props.disabled ? "gray.500" : "gray.200"
                }}
                color={props.disabled ? "gray.500" : "gray.800"}
                _hover={!props.disabled ? activeStyle : {}}
                cursor={props.disabled ? "not-allowed" : undefined}
                {...(props.active && activeStyle)}
                onClick={() => onPaginate(props.page)}
            >
                {props.children}
            </Button>
        );
    };
    if (!current || !count) return <></>

    const prev = current === 1 ? null : current - 1
    const next = current === count ? null : current + 1

    if (!prev && !next) {
        return <></>
    }

    const first = prev == null || prev === 1 ? null : 1
    const last = next == null || next === count ? null : count

    return (
        <Flex
            p={50}
            w="full"
            alignItems="center"
            justifyContent="center"
        >
            <Flex>
                { first ? <PagButton page={first}><ArrowBackIcon boxSize={4} /></PagButton> : <></> }
                { prev ? <PagButton page={prev}>{prev}</PagButton> : <></> }
                <PagButton page={current} active>{current}</PagButton>
                { next ? <PagButton page={next}>{next}</PagButton> : <></> }
                { last ? <PagButton page={last}><ArrowForwardIcon boxSize={4} /></PagButton> : <></> }
            </Flex>
        </Flex>
    );
};
