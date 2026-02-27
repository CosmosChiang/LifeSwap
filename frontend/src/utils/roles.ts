type Translate = (key: string) => string

function normalizeRoleKey(roleName: string): 'administrator' | 'manager' | 'employee' | null {
    const normalized = roleName.trim().toLowerCase()
    if (normalized === 'administrator' || normalized === 'admin') {
        return 'administrator'
    }

    if (normalized === 'manager') {
        return 'manager'
    }

    if (normalized === 'employee') {
        return 'employee'
    }

    return null
}

export function getRoleNameLabel(roleName: string, t: Translate): string {
    const roleKey = normalizeRoleKey(roleName)
    if (!roleKey) {
        return roleName
    }

    return t(`roles.names.${roleKey}`)
}

export function getRoleDescriptionLabel(roleName: string, fallbackDescription: string, t: Translate): string {
    const roleKey = normalizeRoleKey(roleName)
    if (!roleKey) {
        return fallbackDescription
    }

    return t(`roles.descriptions.${roleKey}`)
}
