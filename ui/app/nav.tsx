'use client';

import {
    Box,
    Button,
    Flex,
    HStack, Image,
    IconButton,
    Stack,
    useColorMode,
    useColorModeValue,
    useDisclosure,
} from '@chakra-ui/react'
import {Link} from '@chakra-ui/next-js'
import {CloseIcon, HamburgerIcon, MoonIcon, SunIcon } from '@chakra-ui/icons'
import {usePathname} from "next/navigation";

const NavLink = ({name, href, onClick}: { name: string, href: string, onClick: () => void }) => {
    const pathName = usePathname()
    const bgColor = useColorModeValue('gray.300', 'gray.700')
    return (
        <Link
            px={2}
            py={1}
            rounded={'md'}
            _hover={{
                textDecoration: 'none',
                bg: bgColor,
            }}
            bg={pathName === href ? bgColor : undefined}
            href={href}
            onClick={onClick}>
            {name}
        </Link>
    )
}

export function Nav() {
    const {colorMode, toggleColorMode} = useColorMode()
    const {isOpen, onOpen, onClose} = useDisclosure()

    const navLinks = (<>
        <NavLink name='Movies' href='/movies' onClick={onClose} />
        <NavLink name='Scraper' href='/scraper' onClick={onClose} />
        <NavLink name='Downloads' href='/downloads' onClick={onClose} />
    </>)

    return (
        <>
            <Box bg={useColorModeValue('gray.100', 'gray.900')} px={4}>
                <Flex h={16} alignItems={'center'} justifyContent={'space-between'}>
                    <IconButton
                        size={'md'}
                        icon={isOpen ? <CloseIcon/> : <HamburgerIcon/>}
                        aria-label={'Open Menu'}
                        display={{md: 'none'}}
                        onClick={isOpen ? onClose : onOpen}
                    />

                    <HStack spacing={8} alignItems={'center'}>
                        <Box>
                            <Link href='/'>
                                <Image src='/popcorn.png' boxSize='40px' objectFit='cover' alt='popcorn'  />
                            </Link>
                        </Box>
                        <HStack as={'nav'} spacing={4} display={{base: 'none', md: 'flex'}}>
                            {navLinks}
                        </HStack>
                    </HStack>


                    <Flex alignItems={'center'}>
                        <Stack direction={'row'} spacing={7}>
                            <Button onClick={toggleColorMode}>
                                {colorMode === 'light' ? <MoonIcon/> : <SunIcon/>}
                            </Button>
                        </Stack>
                    </Flex>
                </Flex>

                {isOpen ? (
                    <Box pb={4} display={{md: 'none'}}>
                        <Stack as={'nav'} spacing={4}>
                            {navLinks}
                        </Stack>
                    </Box>
                ) : null}
            </Box>
        </>
    )
}