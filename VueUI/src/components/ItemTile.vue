<template>
    <div class="card border-0 shadow-custom">
        <div class="card-body position-relative">

            <!-- Bookmark Favorite Icon -->
            <i class="bi favorite-icon"
               :class="isFavorite ? 'bi-bookmark-fill' : 'bi-bookmark'"
               :title="isFavorite ? 'Remove from favorites' : 'Add to favorites'"
               @click.stop="toggleFavorite(item)"></i>

            <!-- Relevance Score -->
            <span v-if="showMatch" class="match-label text-muted small">
                {{ relevancePercent }}% match
            </span>

            <!-- Asset type badges top-right -->
            <div class="asset-type-wrapper">
                <span v-for="(type, idx) in item.assetTypes"
                      :key="idx"
                      :class="['badge', getBadgeClass(type), { 'me-1': idx < item.assetTypes.length - 1 }]">
                    {{ type }}
                </span>
            </div>

            <!-- Title -->
            <a href="#" class="title-link d-block text-decoration-none mb-2 mt-1"
               @click.prevent="$emit('show-details')">
                <h5 class="mb-0">{{ item.title }}</h5>
            </a>

            <!-- Description -->
            <p class="card-text text-muted description-asset">
                {{ item.description }}
            </p>

            <!-- Open resource button -->
            <div class="request-access-wrapper">
                <a v-if="item.url"
                   :href="item.url"
                   target="_blank"
                   rel="noopener"
                   class="btn btn-sm btn-outline-teal">
                    Open Resource
                </a>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { computed, toRef } from 'vue';
    import { toggleFavoriteApi } from '../services/api';

    const props = defineProps({
        item: { type: Object, required: true },
        searchExecuted: { type: Boolean, default: false }
    });

    const isFavorite = computed(() => props.item.isFavorite);

    function toggleFavorite(item) {
        toggleFavoriteApi(item.id)
            .then(() => {
                item.isFavorite = !item.isFavorite;
            })
            .catch(err => {
                console.error('Toggle favorite failed', err);
            });
    }

    const relevancePercent = computed(() => {
        const score = props.item.score;
        if (typeof score !== 'number') return null;
        const max = 2.5;
        const normalized = 1 - (score / max);
        return Math.round(normalized * 100);
    });

    const searchExecuted = toRef(props, 'searchExecuted');

    const showMatch = computed(() => {
        return searchExecuted.value && typeof props.item.score === 'number';
    });

    const getBadgeClass = (assetType) => {
        const map = {
            Dashboard: 'bg-dashboard',
            Report: 'bg-report',
            Application: 'bg-application',
            'Data Model': 'bg-data-model',
            Featured: 'bg-featured',
        };
        return map[assetType] || 'bg-teal';
    };
</script>


<style scoped>
    .favorite-icon {
        position: absolute;
        top: -10px;
        left: 10px;
        font-size: 1.4rem;
        color: #ffad44c7;
        cursor: pointer;
        z-index: 15;
    }

        .favorite-icon:hover {
            color: #d78418c7;
        }

    .match-label {
        position: absolute;
        top: -5px;
        left: 40px;
        background-color: #e7f1ff;
        color: #0056b3;
        font-size: 0.75rem;
        font-weight: 500;
        padding: 2px 6px;
        border-radius: 4px;
        border: 1px solid #b6d4fe;
    }

    .card {
        border-radius: 10px;
        min-height: 250px;
        display: flex;
        flex-direction: column;
        position: relative;
    }

    .card-body {
        padding: 1.5rem;
        position: relative;
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .card-title {
        font-size: 1.25rem;
        font-weight: 500;
        margin-bottom: 0;
    }

    .card-text {
        font-size: 0.9rem;
        line-height: 1.4;
        margin-bottom: 0;
        white-space: normal;
        word-wrap: break-word;
    }

    .asset-type-wrapper {
        position: absolute;
        top: 10px;
        right: 10px;
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
        z-index: 20;
    }

    .request-access-wrapper {
        position: absolute;
        bottom: 10px;
        right: 10px;
        z-index: 20;
    }

    .btn-outline-teal {
        border-color: #00A89E;
        color: #00A89E;
        font-size: 0.8rem;
        padding: 0.25rem 0.5rem;
    }

        .btn-outline-teal:hover {
            background-color: #00A89E;
            color: white;
        }

    .badge {
        color: white;
        font-size: 0.65rem;
        padding: 0.3em 0.6em;
    }

    .bg-dashboard {
        background-color: #00A89E;
    }

    .bg-report {
        background-color: #FF6F61;
    }

    .bg-application {
        background-color: #6A5ACD;
    }

    .bg-data-model {
        background-color: #FFA500;
    }

    .bg-featured {
        background-color: #FFD700;
    }

    .bg-teal {
        background-color: #00A89E;
    }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .description-asset {
        max-height: 110px;
        overflow: hidden;
    }

        .description-asset:hover {
            overflow: visible;
        }

    .title-link h5 {
        margin: 0;
        font-size: 1.1rem;
    }

    .title-link p {
        margin: 0;
        font-size: 0.85rem;
    }
</style>
