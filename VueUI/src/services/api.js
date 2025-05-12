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
    return fetch(`${API_BASE_URL}/items?${qs}`).then(handleResponse);
}

/** Fetch a single item by its ID */
export function fetchItem(id) {
    return fetch(`${API_BASE_URL}/items/${id}`).then(handleResponse);
}

/** Create a new item */
export function createItem(payload) {
    return fetch(`${API_BASE_URL}/items`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    }).then(handleResponse);
}

/** Update an existing item */
export function updateItem(id, payload) {
    return fetch(`${API_BASE_URL}/items/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    }).then(handleResponse);
}

/** Delete an item */
export function deleteItem(id) {
    return fetch(`${API_BASE_URL}/items/${id}`, {
        method: 'DELETE',
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
    return fetch(`${API_BASE_URL}/lookups/${type}`).then(handleResponse);
}

/** Search items by full-text query (parameter `q`) */
export function searchItems(q) {
    return fetch(`${API_BASE_URL}/search?q=${encodeURIComponent(q)}`)
        .then(handleResponse);
}

