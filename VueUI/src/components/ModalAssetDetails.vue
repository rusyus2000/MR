<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow">
            <div class="modal-header custom-header">
                <h5 class="modal-title d-flex align-items-center gap-2">
                    <i v-if="!isLoading && item && item.featured" class="bi bi-star-fill featured-icon" title="Featured"></i>
                    <span>{{ isLoading ? 'Loading…' : (item ? item.title : '') }}</span>
                    <button v-if="!isLoading && isAdmin" class="btn btn-link text-primary p-0 ms-2" title="Edit Item" @click="$emit('edit')">
                        <i class="bi bi-pencil-square fs-4"></i>
                    </button>
                </h5>
                <div class="d-flex align-items-center gap-2">
                    <button class="btn-close" @click="$emit('close')"></button>
                </div>
            </div>
            <div class="modal-body">
                <div v-if="isLoading" class="d-flex justify-content-center align-items-center" style="min-height:160px;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
                <div v-else class="details-grid">
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

                    <div class="label">Owner:</div>
                    <div>
                        <template v-if="item.ownerEmail">
                            <a :href="`mailto:${item.ownerEmail}`">{{ item.ownerName || item.ownerEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.ownerName || '-' }}
                        </template>
                    </div>

                    <div class="label">Executive Sponsor:</div>
                    <div>
                        <template v-if="item.executiveSponsorEmail">
                            <a :href="`mailto:${item.executiveSponsorEmail}`">{{ item.executiveSponsorName || item.executiveSponsorEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.executiveSponsorName || '-' }}
                        </template>
                    </div>

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

                    <div class="label">Dependencies:</div>
                    <div><span v-if="item.dependencies && item.dependencies.trim()">{{ item.dependencies }}</span><span v-else class="text-muted">-</span></div>

                    <div class="label">Default AD Groups:</div>
                    <div><span v-if="item.defaultAdGroupNames && item.defaultAdGroupNames.trim()">{{ item.defaultAdGroupNames }}</span><span v-else class="text-muted">-</span></div>

                    <div class="label">Tags:</div>
                    <div>
                        <div class="tag-list">
                            <template v-if="!item || !item.tags || item.tags.length === 0">
                                <span class="text-muted">None</span>
                            </template>
                            <template v-else>
                                <span v-for="(t, idx) in item.tags" :key="idx" class="tag-chip me-1">
                                    <span class="tag-text">{{ t }}</span>
                                </span>
                            </template>
                        </div>
                    </div>

                    <div class="label">Date Added:</div><div>{{ new Date(item.dateAdded).toLocaleDateString() }}</div>
                </div>
            </div>
            <div v-if="!isLoading && item" class="d-flex justify-content-end mt-3 px-4 pb-3">
                <button class="btn btn-sm favorite-icon-btn" @click="toggleFavorite(item)">
                    {{ item.isFavorite ? '★ Remove from Favorites' : '☆ Add to Favorites' }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import { toggleFavoriteApi } from '../services/api';

    export default {
        name: 'ModalAssetDetails',
        props: {
            item: Object,
            isAdmin: { type: Boolean, default: false },
            isLoading: { type: Boolean, default: false }
        },
        emits: ['close','edit'],
        methods: {
            async toggleFavorite(item) {
                if (!item) return;
                try {
                    await toggleFavoriteApi(item.id);
                    item.isFavorite = !item.isFavorite;
                } catch (err) {
                    console.error('Failed to toggle favorite', err);
                }
            },

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

    .modal-content {
        background-color: white;
        border-radius: 8px;
        padding: 0;
        max-width: 900px;
        width: 100%;
    }

    .modal-header.custom-header {
        background-color: #f0f4f8;
        padding: 1rem 1.5rem;
        border-bottom: 1px solid #dee2e6;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-top-left-radius: 8px;
        border-top-right-radius: 8px;
    }

    .modal-body { padding: 1.5rem; max-height: 75vh; overflow: auto; }

    .details-grid {
        display: grid;
        grid-template-columns: max-content 1fr max-content 1fr;
        row-gap: 0.75rem;
        column-gap: 1rem;
    }

    /* Make value cells align items consistently (center vertically) so multi-line
       content like tag chips aligns the same as other property values. */
    .details-grid > div {
        display: flex;
        align-items: center;
    }

    .label {
        font-weight: 600;
        white-space: nowrap;
    }

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

    .tag-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        margin: 0; /* ensure no extra vertical spacing */
        align-items: center;
    }

    .tag-chip {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.25rem 0.5rem;
        border-radius: 0.35rem;
        background-color: #eaf6ff;
        border: 1px solid #c7e6ff;
        color: #05567a;
        font-size: 0.85rem;
    }

    .tag-chip .tag-text {
        line-height: 1;
        display: inline-block;
        vertical-align: middle;
    }

    /* Styled like your existing favorite icon */
    .favorite-icon-btn {
        color: #ffad44c7;
        border-color: #ffad44c7;
        background: transparent;
    }

    .favorite-icon-btn:hover {
            color: #d78418c7;
            border-color: #d78418c7;
        }

    .featured-icon {
        color: #FFD700;
        font-size: 0.95rem;
        vertical-align: text-bottom;
    }
</style>
