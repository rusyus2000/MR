<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow">
            <div class="modal-header">
                <h5 class="modal-title">
                    <i v-if="item && item.featured" class="bi bi-star-fill featured-icon me-1" title="Featured"></i>
                    {{ item.title }}
                </h5>
                <div class="d-flex align-items-center gap-2">
                    <button v-if="isAdmin" class="btn btn-sm btn-outline-secondary" title="Edit" @click="$emit('edit')">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn-close" @click="$emit('close')"></button>
                </div>
            </div>
            <div class="modal-body">
                <div class="details-grid">
                    <div class="label">Description:</div>
                    <div class="desc" :title="item.description">{{ shorten(item.description, 65) }}</div>
                    <div class="label">URL:</div>
                    <div class="url-row">
                        <a href="#" @click.prevent="openResource(item)" :title="item.url">
                            {{ shorten(item.url, 30) }}
                        </a>
                        <button class="btn btn-link btn-sm p-0 ms-2 copy-btn" :title="'Copy URL'" @click="copyUrl(item.url)">
                            <i class="bi bi-clipboard"></i>
                        </button>
                    </div>
                    <div class="label">Asset Type:</div><div>{{ item.assetTypeName }}</div>
                    <div class="label">Status:</div><div>{{ item.status || '-' }}</div>
                    <div class="label">Owner:</div><div>{{ item.ownerName }}<span v-if="item.ownerEmail"> ({{ item.ownerEmail }})</span></div>
                    <div class="label">Executive Sponsor:</div><div>{{ item.executiveSponsorName }}<span v-if="item.executiveSponsorEmail"> ({{ item.executiveSponsorEmail }})</span></div>
                    <div class="label">Domain:</div><div>{{ item.domain }}</div>
                    <div class="label">Division:</div><div>{{ item.division }}</div>
                    <div class="label">Service Line:</div><div>{{ item.serviceLine }}</div>
                    <div class="label">Operating Entity:</div><div>{{ item.operatingEntity || '-' }}</div>
                    <div class="label">Data Source:</div><div>{{ item.dataSource }}</div>
                    <div class="label">Refresh Frequency:</div><div>{{ item.refreshFrequency || '-' }}</div>
                    <div class="label">Last Modified:</div><div>{{ item.lastModifiedDate ? new Date(item.lastModifiedDate).toLocaleDateString() : '-' }}</div>
                    <div class="label">Flags:</div>
                    <div class="flags">
                        <span>PHI: {{ item.privacyPhiDisplay || (item.privacyPhi === null || item.privacyPhi === undefined ? 'Missing Data' : (item.privacyPhi ? 'Yes' : 'No')) }}</span>
                        <span>PII: {{ item.privacyPiiDisplay || (item.privacyPii === null || item.privacyPii === undefined ? 'Missing Data' : (item.privacyPii ? 'Yes' : 'No')) }}</span>
                        <span>RLS: {{ item.hasRlsDisplay || (item.hasRls === null || item.hasRls === undefined ? 'Missing Data' : (item.hasRls ? 'Yes' : 'No')) }}</span>
                    </div>
                    <div class="label">Data Consumers:</div><div><span v-if="item.dataConsumers && item.dataConsumers.length">{{ item.dataConsumers.join(', ') }}</span><span v-else>-</span></div>
                    <div class="label">Dependencies:</div><div><span v-if="item.dependencies && item.dependencies.trim()">{{ item.dependencies }}</span><span v-else class="text-muted">-</span></div>
                    <div class="label">Default AD Groups:</div><div><span v-if="item.defaultAdGroupNames && item.defaultAdGroupNames.trim()">{{ item.defaultAdGroupNames }}</span><span v-else class="text-muted">-</span></div>
                    <div class="label">Date Added:</div><div>{{ new Date(item.dateAdded).toLocaleDateString() }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'ModalAssetDetails',
        props: {
            item: Object,
            isAdmin: { type: Boolean, default: false }
        },
        emits: ['close','edit'],
        methods: {
            async openResource(item) {
                try {
                    const api = await import('../services/api');
                    await api.recordOpen(item.id);
                } catch (err) {
                    console.error('record open failed', err);
                }
                window.open(item.url, '_blank', 'noopener');
            },
            shorten(s, max = 30) {
                if (!s || typeof s !== 'string') return '';
                return s.length > max ? s.slice(0, max) + '...' : s;
            },
            async copyUrl(url) {
                if (!url) return;
                try {
                    if (navigator.clipboard && navigator.clipboard.writeText) {
                        await navigator.clipboard.writeText(url);
                    } else {
                        const ta = document.createElement('textarea');
                        ta.value = url;
                        ta.style.position = 'fixed';
                        ta.style.opacity = '0';
                        document.body.appendChild(ta);
                        ta.select();
                        document.execCommand('copy');
                        document.body.removeChild(ta);
                    }
                } catch (e) {
                    console.error('copy failed', e);
                }
            }
        }
    };
</script>

<style scoped>
    .modal-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1050;
    }

    .modal-content { background-color: white; border-radius: 8px; padding: 1.5rem; max-width: 900px; width: 100%; }
    .modal-body { max-height: 75vh; overflow: auto; }
    .details-grid { display: grid; grid-template-columns: max-content 1fr max-content 1fr; row-gap: 0.75rem; column-gap: 1rem; }
    .label { font-weight: 600; white-space: nowrap; }
    .flags { display: flex; gap: 1rem; align-items: center; }

    .url-row { display: inline-flex; align-items: center; gap: 0.25rem; }
    .copy-btn { color: #0d6efd; }
    .copy-btn:hover { color: #0a58ca; }

    /* Two-line clamp with ellipsis and fixed height */
    .clamp-2 {
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        white-space: normal;
        line-height: 1.35;
        max-height: calc(1.35em * 2);
        min-height: calc(1.35em * 2);
    }
    .desc { max-width: 100%; }

    .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1rem;
    }

    .featured-icon {
        color: #FFD700;
        font-size: 0.95rem;
        vertical-align: text-bottom;
    }
</style>
