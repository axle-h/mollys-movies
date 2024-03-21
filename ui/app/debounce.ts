import {useEffect, useRef} from 'react';

export default function useDebounce<T extends Array<any>>(callback: (...args: T) => void | Promise<void>, delay: number) {
    const timeoutRef = useRef<NodeJS.Timeout | null>(null)

    useEffect(() => {
        // Cleanup the previous timeout on re-render
        return () => {
            if (timeoutRef.current) {
                clearTimeout(timeoutRef.current);
            }
        }
    }, [])

    return (...args: T) => {
        if (timeoutRef.current) {
            clearTimeout(timeoutRef.current)
        }

        timeoutRef.current = setTimeout(() => callback(...args), delay)
    }
};