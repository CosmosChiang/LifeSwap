export type FetchCall = [input: RequestInfo | URL, init?: RequestInit]

export function requireFirstFetchCall(calls: FetchCall[]): FetchCall {
    const firstCall = calls[0]
    if (!firstCall) {
        throw new Error('Expected fetch to be called at least once.')
    }

    return firstCall
}

export function toRequestUrl(input: RequestInfo | URL): string {
    if (typeof input === 'string') {
        return input
    }

    if (input instanceof URL) {
        return input.toString()
    }

    return input.url
}

export function requireRequestInit(init: RequestInit | undefined): RequestInit {
    if (!init) {
        throw new Error('Expected fetch init options.')
    }

    return init
}
