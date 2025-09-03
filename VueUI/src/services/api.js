// src/services/api.js
import { API_BASE_URL } from '../config';

async function handleResponse(res) {
    if (!res.ok) {
        const text = await res.text();
        throw new Error(`API error ${res.status}: ${text}`);
    }
    return res.json();
}

/**
 * Fetch all items, with optional query parameters:
 *   q, domain, division, serviceLine, dataSource, assetType, phi (boolean), top (number)
 */
export function fetchItems(params = {}) {
    const qs = new URLSearchParams(params).toString();
    return fetch(`${API_BASE_URL}/items?${qs}`, { credentials: 'include' }).then(handleResponse);
}

/** Fetch a single item by its ID */
export function fetchItem(id) {
    return fetch(`${API_BASE_URL}/items/${id}`, { credentials: 'include' }).then(handleResponse);
}

/** Create a new item */
//export function createItem(payload) {
//    return fetch(`${API_BASE_URL}/items`, {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//        },
//        credentials: 'include', // This is required for Windows Auth
//        body: JSON.stringify(payload),
//    })
//    .then(handleResponse);
//}
export function createItem(payload) {
    const form = new FormData();
    for (const key in payload) {
        const value = payload[key];
        if (Array.isArray(value)) {
            value.forEach(v => form.append(key, v));
        } else {
            form.append(key, value);
        }
    }

    return fetch(`${API_BASE_URL}/items`, {
        method: 'POST',
        credentials: 'include',
        body: form // ✅ no headers needed
    }).then(handleResponse);
}

/** Update an existing item */
export function updateItem(id, payload) {
    return fetch(`${API_BASE_URL}/items/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(payload),
    }).then(handleResponse);
}

/** Delete an item */
export function deleteItem(id) {
    return fetch(`${API_BASE_URL}/items/${id}`, {
        method: 'DELETE',
        credentials: 'include'
    }).then(res => {
        if (!res.ok) throw new Error(`Delete failed: ${res.status}`);
    });
}

/**
 * Fetch lookup values for dropdowns.
 * type should be one of:
 *   "Domain", "Division", "ServiceLine", "DataSource", "AssetType"
 */
export function fetchLookup(type) {
    return fetch(`${API_BASE_URL}/lookups/${type}`, { credentials: 'include' }).then(handleResponse);
}

export function fetchLookupWithCounts(type) {
    return fetch(`${API_BASE_URL}/lookups/${type}/counts`, { credentials: 'include' }).then(handleResponse);
}

/** Search items by full-text query (parameter `q`) */
export function searchItems(q) {
    return fetch(`${API_BASE_URL}/search?q=${encodeURIComponent(q)}`, { credentials: 'include' })
        .then(handleResponse);
}

export function toggleFavoriteApi(itemId) {
    return fetch(`${API_BASE_URL}/useractions/togglefavorite/${itemId}`, {
        method: 'POST',
        credentials: 'include'
    }).then(res => {
        if (!res.ok && res.status !== 204) {
            throw new Error(`Toggle failed: ${res.status}`);
        }
    });
}

/** Record that a user opened an asset (opens are logged server-side) */
export function recordOpen(itemId) {
    return fetch(`${API_BASE_URL}/useractions/recordopen/${itemId}`, {
        method: 'POST',
        credentials: 'include'
    }).then(res => {
        if (!res.ok && res.status !== 204) {
            throw new Error(`Record open failed: ${res.status}`);
        }
    });
}

export async function fetchFavorites() {
    return fetch(`${API_BASE_URL}/useractions/favorites`, {
        credentials: 'include'
    }).then(res => {
        if (!res.ok) throw new Error('Failed to fetch favorites');
        return res.json();
    });
}

// Owners
export function searchOwners(search, top = 10) {
    const qs = new URLSearchParams({ search: search || '', top });
    return fetch(`${API_BASE_URL}/owners?${qs}`, { credentials: 'include' }).then(handleResponse);
}

// Current user (to detect admins)
export function fetchCurrentUser() {
    return fetch(`${API_BASE_URL}/users/me`, { credentials: 'include' }).then(res => {
        if (res.status === 401) return null;
        return res.json();
    });
}
