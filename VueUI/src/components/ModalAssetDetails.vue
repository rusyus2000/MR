<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow">
            <div class="modal-header custom-header">
                <h5 class="modal-title">{{ item.title }}</h5>
                <button class="btn-close" @click="$emit('close')"></button>
            </div>
            <div class="modal-body">
                <div class="details-grid">
                    <div class="label">Description:</div><div>{{ item.description }}</div>
                    <div class="label">URL:</div><div><a href="#" @click.prevent="openResource(item)">{{ item.url }}</a></div>
                    <div class="label">Asset Types:</div><div>{{ item.assetTypes.join(', ') }}</div>
                    <div class="label">Domain:</div><div>{{ item.domain }}</div>
                    <div class="label">Division:</div><div>{{ item.division }}</div>
                    <div class="label">Service Line:</div><div>{{ item.serviceLine }}</div>
                    <div class="label">Data Source:</div><div>{{ item.dataSource }}</div>
                    <div class="label">Contains PHI:</div><div>{{ item.privacyPhi ? 'Yes' : 'No' }}</div>
                    <div class="label">Date Added:</div><div>{{ new Date(item.dateAdded).toLocaleDateString() }}</div>
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
                </div>
            </div>
            <div class="d-flex justify-content-end mt-3 px-4 pb-3">
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
            item: Object
        },
        methods: {
            async toggleFavorite(item) {
                try {
                    await toggleFavoriteApi(item.id);
                    item.isFavorite = !item.isFavorite;
                } catch (err) {
                    console.error('Failed to toggle favorite', err);
                }
            }
            ,
            async openResource(item) {
                try {
                    const api = await import('../services/api');
                    await api.recordOpen(item.id);
                } catch (err) {
                    console.error('record open failed', err);
                }
                window.open(item.url, '_blank', 'noopener');
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
        max-width: 600px;
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

    .modal-body {
        padding: 1.5rem;
    }

    .details-grid {
        display: grid;
        grid-template-columns: max-content 1fr;
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
</style>
