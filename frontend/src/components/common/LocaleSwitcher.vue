<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { getLocale, setLocale, type SupportedLocale } from '../../i18n'

const { t } = useI18n()

const currentLocale = computed(() => getLocale())

function handleSwitch(locale: SupportedLocale) {
  setLocale(locale)
}
</script>

<template>
  <div class="locale-switcher" role="group" :aria-label="t('locale.en')">
    <button
      type="button"
      class="locale-btn"
      :class="{ 'locale-btn--active': currentLocale === 'zh-TW' }"
      @click="handleSwitch('zh-TW')"
    >
      {{ t('locale.zhTw') }}
    </button>
    <button
      type="button"
      class="locale-btn"
      :class="{ 'locale-btn--active': currentLocale === 'en' }"
      @click="handleSwitch('en')"
    >
      {{ t('locale.en') }}
    </button>
  </div>
</template>

<style scoped>
.locale-switcher {
  display: inline-flex;
  align-items: center;
  border: 1px solid #c7d2fe;
  background: #eef2ff;
  border-radius: 10px;
  padding: 2px;
}

.locale-btn {
  border: none;
  background: transparent;
  color: #3730a3;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 8px;
  border-radius: 8px;
  cursor: pointer;
}

.locale-btn--active {
  background: #4f46e5;
  color: #fff;
}
</style>
