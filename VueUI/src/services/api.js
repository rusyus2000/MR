// src/services/api.js
import { API_BASE_URL } from '../config';

async function handleResponse(res) {
    if (!res.ok) {
        const text = await res.text();
        throw new Error(`API error ${res.status}: ${text}`);
    }
    if (res.status === 204) return null;
    const ct = res.headers.get('content-type') || '';
    if (!ct.toLowerCase().includes('application/json')) {
        // No JSON body; return null to indicate success without payload
        return null;
    }
    // Some servers may send empty body with JSON content-type; guard it
    const text = await res.text();
    if (!text) return null;
    try { return JSON.parse(text); } catch { return null; }
}

/**
 * Fetch all items, with optional query parameters:
 *   q, domain, division, serviceLine, dataSource, assetType, phi (boolean), top (number)
 */
export function fetchItems(params = {}) {
    const usp = new URLSearchParams();
    for (const [k, v] of Object.entries(params || {})) {
        if (v === undefined || v === null) continue;
        const s = String(v);
        if (!s || s === 'undefined') continue;
        usp.append(k, s);
    }
    const qs = usp.toString();
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
    const appendField = (k, v) => {
        if (v === null || v === undefined) return; // skip null/undefined to avoid sending literal "null"
        if (typeof v === 'string') {
            const t = v.trim();
            if (t.length === 0) return; // skip empty strings
            form.append(k, t);
            return;
        }
        if (typeof v === 'number' && Number.isFinite(v)) {
            form.append(k, String(v));
            return;
        }
        if (typeof v === 'boolean') {
            form.append(k, v ? 'true' : 'false');
            return;
        }
        // arrays
        if (Array.isArray(v)) {
            v.forEach((entry) => appendField(k, entry));
            return;
        }
        // fallback: stringify
        form.append(k, String(v));
    };

    Object.keys(payload).forEach((key) => appendField(key, payload[key]));

    return fetch(`${API_BASE_URL}/items`, {
        method: 'POST',
        credentials: 'include',
        body: form // ✅ no headers needed
    }).then(handleResponse);
}

/** Update an existing item */
export function updateItem(id, payload) {
    // Use FormData to avoid CORS preflight (multipart/form-data is a simple content-type)
    const form = new FormData();
    const appendField = (k, v) => {
        if (v === null || v === undefined) return;
        if (typeof v === 'string') {
            const t = v.trim();
            if (t.length === 0) return;
            form.append(k, t);
            return;
        }
        if (typeof v === 'number' && Number.isFinite(v)) { form.append(k, String(v)); return; }
        if (typeof v === 'boolean') { form.append(k, v ? 'true' : 'false'); return; }
        if (Array.isArray(v)) { v.forEach(entry => appendField(k, entry)); return; }
        form.append(k, String(v));
    };
    Object.keys(payload || {}).forEach(key => appendField(key, payload[key]));
    // Use a POST compatibility route to avoid CORS preflight on IIS Express
    return fetch(`${API_BASE_URL}/items/${id}/edit`, {
        method: 'POST',
        credentials: 'include',
        body: form,
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

export function fetchLookupsBulk(types = ['AssetType','Domain','Division','ServiceLine','DataSource','Status']) {
    const qs = new URLSearchParams({ types: types.join(',') }).toString();
    return fetch(`${API_BASE_URL}/lookups/bulk?${qs}`, { credentials: 'include' }).then(handleResponse);
}

// Cached bulk lookups (by types key)
const __bulkLookupsCache = new Map();
export function getLookupsBulkCached(types = ['AssetType','Domain','Division','ServiceLine','DataSource','Status'], force = false) {
    const key = (types || []).map(t => String(t).toLowerCase()).sort().join(',');
    if (!force && __bulkLookupsCache.has(key)) {
        return __bulkLookupsCache.get(key);
    }
    const p = fetchLookupsBulk(types);
    __bulkLookupsCache.set(key, p);
    return p;
}

export function fetchLookupsCountsBulk(types = ['AssetType','Domain','Division','ServiceLine','DataSource','Status']) {
    const qs = new URLSearchParams({ types: types.join(',') }).toString();
    return fetch(`${API_BASE_URL}/lookups/bulk-counts?${qs}`, { credentials: 'include' }).then(handleResponse);
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

// Export items (admin only). Sends IDs and downloads CSV.
export async function exportItemsCsv(ids) {
    let fetchOptions = { method: 'POST', credentials: 'include' };
    if (Array.isArray(ids) && ids.length > 0) {
        const form = new FormData();
        ids.forEach(id => form.append('ids', String(id)));
        fetchOptions.body = form;
    }
    const res = await fetch(`${API_BASE_URL}/items/export`, fetchOptions);
    if (!res.ok) throw new Error('Export failed');
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'reports-export.csv';
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}

// Import preview (admin): multipart form with file; returns token + counts + conflicts/errors
export async function importPreview(file, { source = '', datasetKey = '', mode = 'upsert' } = {}) {
    const form = new FormData();
    form.append('file', file);
    if (source) form.append('source', source);
    if (datasetKey) form.append('datasetKey', datasetKey);
    if (mode) form.append('mode', mode);
    const res = await fetch(`${API_BASE_URL}/imports/preview`, {
        method: 'POST',
        credentials: 'include',
        body: form
    });
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || 'Preview failed');
    }
    return res.json();
}

// Import commit (admin): send token + decisions
export async function importCommit(token, decisions = []) {
    // Use FormData to avoid CORS preflight
    const form = new FormData();
    form.append('token', token);
    form.append('decisions', JSON.stringify(decisions || []));
    const res = await fetch(`${API_BASE_URL}/imports/commit`, {
        method: 'POST',
        credentials: 'include',
        body: form
    });
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || 'Commit failed');
    }
    return res.json();
}

// Trigger a full search index rebuild (server makes the external call)
export async function importRebuild() {
    const res = await fetch(`${API_BASE_URL}/imports/rebuild-index`, {
        method: 'POST',
        credentials: 'include'
    });
    if (!res.ok && res.status !== 204) {
        const text = await res.text();
        throw new Error(text || 'Rebuild failed');
    }
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

// In-memory cached current user to avoid duplicate calls per page
let __meCachePromise;
export function getCurrentUserCached(force = false) {
    if (force || !__meCachePromise) {
        __meCachePromise = fetchCurrentUser();
    }
    return __meCachePromise;
}

// Intranet user API (Windows-auth) called from browser
export function fetchIntranetUser() {
    return fetch('http://fbs.sutterhealth.org/user_api/user/getusername', {
        credentials: 'include'
    }).then(res => res.ok ? res.json() : null).catch(() => null);
}

export function updateCurrentUserProfile({ displayName, email, networkId }) {
    const form = new FormData();
    if (displayName) form.append('DisplayName', displayName);
    if (email) form.append('Email', email);
    if (networkId) form.append('NetworkId', networkId);
    return fetch(`${API_BASE_URL}/users/profile`, {
        method: 'POST',
        credentials: 'include',
        body: form
    }).then(res => { if (!res.ok && res.status !== 204) throw new Error('Failed to update profile'); });
}
