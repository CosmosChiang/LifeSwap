import { config } from '@vue/test-utils'
import { i18n } from '../i18n'

// Polyfill window.matchMedia for jsdom (required by Ant Design Vue)
Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: () => { },
        removeListener: () => { },
        addEventListener: () => { },
        removeEventListener: () => { },
        dispatchEvent: () => false,
    }),
})

config.global.plugins = [...(config.global.plugins ?? []), i18n]
