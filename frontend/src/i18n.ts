import { createI18n } from 'vue-i18n'
import en from './locales/en'
import zhTw from './locales/zh-TW'

const LOCALE_STORAGE_KEY = 'app_locale'

type SupportedLocale = 'zh-TW' | 'en'

const messages = {
    'zh-TW': zhTw,
    en,
}

function resolveInitialLocale(): SupportedLocale {
    const saved = localStorage.getItem(LOCALE_STORAGE_KEY)
    if (saved === 'zh-TW' || saved === 'en') {
        return saved
    }

    return 'zh-TW'
}

export const i18n = createI18n({
    legacy: false,
    globalInjection: true,
    locale: resolveInitialLocale(),
    fallbackLocale: 'en',
    messages,
})

export function setLocale(locale: SupportedLocale) {
    i18n.global.locale.value = locale
    localStorage.setItem(LOCALE_STORAGE_KEY, locale)
}

export function getLocale(): SupportedLocale {
    return i18n.global.locale.value as SupportedLocale
}

export type { SupportedLocale }
