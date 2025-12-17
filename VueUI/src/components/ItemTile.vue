<template>
    <div class="card border-0 shadow-custom">
        <div class="card-body position-relative">

            <!-- Bookmark Favorite Icon -->
            <i class="bi favorite-icon"
               :class="isFavorite ? 'bi-bookmark-fill' : 'bi-bookmark'"
               :title="isFavorite ? 'Remove from favorites' : 'Add to favorites'"
               @click.stop="toggleFavorite(item)"></i>

            <!-- Relevance Score -->
            <span v-if="showMatch" class="match-label text-muted small" :style="matchStyle">
                {{ relevancePercent }}% match
            </span>

            <!-- Asset type badges top-right -->
            <div class="asset-type-wrapper">
                <span v-if="item.assetTypeName"
                      class="badge"
                      :class="getBadgeClass(item.assetTypeName)">
                    {{ item.assetTypeName }}
                </span>
                <span v-else class="badge" :class="getBadgeClass(item.assetTypeName)">
                    {{ item.assetTypeName }}
                </span>
            </div>

            <!-- Title -->
            <a href="#" class="title-link d-block text-decoration-none mb-2 mt-1"
               @click.prevent="$emit('show-details')">
                <h5 class="mb-0">
                    {{ item.title }}
                </h5>
            </a>

            <!-- Description -->
            <p class="card-text text-muted description-asset">
                {{ item.description }}
            </p>

            <!-- Open resource button -->
            <div class="request-access-wrapper">
                <a v-if="item.url"
                   href="#"
                   class="btn btn-sm btn-outline-teal"
                   @click.prevent="openResource(item)">
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
        // New scale: score in [0,1], higher is better
        const clamped = Math.max(0, Math.min(1, score));
        return Math.round(clamped * 100);
    });

    const searchExecuted = toRef(props, 'searchExecuted');

    const showMatch = computed(() => {
        return searchExecuted.value && typeof props.item.score === 'number';
    });

    const matchStyle = computed(() => {
        const p = relevancePercent.value;
        if (p == null) return {};
        // Quantize to 5% steps
        const step = Math.max(0, Math.min(100, Math.round(p / 5) * 5));
        // Map 0% -> red (hue=0), 100% -> green (hue=120)
        const hue = Math.round((step / 100) * 120);
        // Solid color for border and text decision
        const solid = `hsl(${hue}deg 90% 40%)`;
        // Background: same color at 10% opacity
        const bgWithAlpha = `hsl(${hue}deg 90% 40% / 0.1)`;
        // Choose text color for contrast based on lightness (40 -> use white)
        const textColor = 40 >= 50 ? '#000' : '#fff';
        return {
            backgroundColor: bgWithAlpha,
            borderColor: solid
        };
    });

    const getBadgeClass = (assetType) => {
        const map = {
            Dashboard: 'bg-dashboard',
            Report: 'bg-report',
            Application: 'bg-application',
            'Data Model': 'bg-data-model',
        };
        return map[assetType] || 'bg-teal';
    };

    async function openResource(item) {
        try {
            // best-effort record open
            await (await import('../services/api')).recordOpen(item.id);
        } catch (err) {
            // ignore failures to record
            console.error('record open failed', err);
        }
        window.open(item.url, '_blank', 'noopener');
    }
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
        color: var(--bs-secondary-color) !important;
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
