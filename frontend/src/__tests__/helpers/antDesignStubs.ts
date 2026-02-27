export const antDesignVueShallowStubs = {
    'a-card': {
        props: ['title', 'loading'],
        template: '<div class="a-card" :data-title="title"><slot /></div>',
    },
    'a-list': {
        props: ['dataSource', 'bordered'],
        template:
            '<ul><template v-for="item in dataSource" :key="item.id"><slot name="renderItem" :item="item" /></template></ul>',
    },
    'a-list-item': {
        template: '<li><slot /><slot name="actions" /></li>',
    },
    'a-list-item-meta': {
        template: '<div><slot name="title" /><slot name="description" /></div>',
    },
    'a-button': {
        props: ['type', 'size'],
        emits: ['click'],
        template: '<button @click="$emit(\'click\')"><slot /></button>',
    },
    'a-tag': {
        props: ['color'],
        template: '<span><slot /></span>',
    },
    'a-row': {
        template: '<div class="a-row"><slot /></div>',
    },
    'a-col': {
        template: '<div class="a-col"><slot /></div>',
    },
    'a-form-item': {
        props: ['label'],
        template: '<div class="a-form-item" :data-label="label"><slot /></div>',
    },
    'a-date-picker': {
        props: ['value'],
        template: '<div class="a-date-picker" />',
    },
    'a-select': {
        props: ['value', 'placeholder', 'allowClear'],
        emits: ['update:value'],
        template:
            '<select :value="value" @change="$emit(\'update:value\', $event.target.value)"><slot /></select>',
    },
    'a-select-option': {
        props: ['value'],
        template: '<option :value="value"><slot /></option>',
    },
    'a-input': {
        props: ['value', 'placeholder'],
        emits: ['update:value'],
        template:
            '<input :value="value" :placeholder="placeholder" @input="$emit(\'update:value\', $event.target.value)" />',
    },
    'a-input-number': {
        props: ['value', 'min'],
        emits: ['update:value'],
        template:
            '<input type="number" :value="value" @input="$emit(\'update:value\', Number($event.target.value))" />',
    },
    'a-alert': {
        props: ['message', 'type', 'showIcon', 'closable'],
        emits: ['close'],
        template: '<div class="a-alert" :data-message="message"><slot /></div>',
    },
    'a-statistic': {
        props: ['title', 'value', 'suffix', 'valueStyle'],
        template: '<div class="a-statistic" :data-title="title" :data-value="value" :data-suffix="suffix" />',
    },
    'a-table': {
        props: ['columns', 'dataSource', 'rowKey', 'pagination', 'size', 'bordered'],
        template: '<div class="a-table" />',
    },
    'a-empty': {
        props: ['description'],
        template: '<div class="a-empty" :data-description="description" />',
    },
} as const

export function createAntDesignMountOptions() {
    return {
        global: {
            stubs: antDesignVueShallowStubs,
        },
    }
}
