'use client';

import {CheckIcon, CloseIcon} from "@chakra-ui/icons";

export function BoolIcon({ value }: { value: boolean }) {
    return value ? <CheckIcon color='green'/> : <CloseIcon color='red'/>;
}
