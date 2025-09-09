<template>
    <div class="card list-item-card shadow-custom mb-1">
        <div class="card-body d-flex align-items-center p-2 position-relative">
            <!-- Bookmark Favorite Icon -->
            <i class="bi favorite-icon"
               :class="isFavorite ? 'bi-bookmark-fill' : 'bi-bookmark'"
               :title="isFavorite ? 'Remove from favorites' : 'Add to favorites'"
               @click.stop="toggleFavorite(item)"></i>

            <!-- Relevance Score below bookmark -->
            <span v-if="searchExecuted && item.score !== undefined"
                  class="match-score-badge"
                  :style="matchStyle">
                {{ relevancePercent }}<small>%</small>
            </span>

            <!-- Title & description -->
            <div class="flex-grow-1 ps-4">
                <a href="#" @click.prevent="$emit('show-details')" class="text-decoration-none">
                    <h5 class="mb-0 card-title">
                        <i v-if="isFeatured" class="bi bi-star-fill featured-icon" title="Featured"></i>
                        {{ item.title }}
                    </h5>
                </a>
                <p class="mb-0 text-muted small">{{ truncatedDescription }}</p>
            </div>


            <!-- Asset type badge -->
            <span v-if="item.assetTypeName" class="badge mx-2" :class="getBadgeClass(item.assetTypeName)">
                {{ item.assetTypeName }}
            </span>

            <!-- External link icon -->
            <a v-if="item.url"
               href="#"
               class="link-icon"
               title="Open Resource"
               @click.prevent="openResource(item)">
                <i class="bi bi-box-arrow-up-right"></i>
            </a>
        </div>
    </div>
</template>

<script>
    import { computed } from 'vue';
    import { toggleFavoriteApi } from '../services/api';

    export default {
        name: 'ListItem',
        props: {
            item: {
                type: Object,
                required: true
            },
            searchExecuted: {
                type: Boolean,
                default: false
            }
        },
    computed: {
            truncatedDescription() {
                const max = 90;
                return this.item.description.length > max
                    ? this.item.description.slice(0, max) + '...'
                    : this.item.description;
            },
            isFavorite() {
                return this.item.isFavorite;
            },
            isFeatured() {
                return !!this.item.featured;
            },
            relevancePercent() {
                if (typeof this.item.score !== 'number') return null;
                const clamped = Math.max(0, Math.min(1, this.item.score));
                return Math.round(clamped * 100);
            },
            matchStyle() {
                const p = this.relevancePercent;
                if (p == null) return {};
                const step = Math.max(0, Math.min(100, Math.round(p / 5) * 5));
                const hue = Math.round((step / 100) * 120);
                const solid = `hsl(${hue}deg 90% 40%)`;
                const bgWithAlpha = `hsl(${hue}deg 90% 40% / 0.1)`;
                const textColor = 40 >= 50 ? '#000' : '#fff';
                return {
                    backgroundColor: bgWithAlpha,
                    borderColor: solid
                };
            },
            showMatch() {
                return this.searchExecuted && typeof this.item.score === 'number';
            }
        },
        methods: {
            async toggleFavorite(item) {
                try {
                    await toggleFavoriteApi(item.id);  //  fixed function name
                    item.isFavorite = !item.isFavorite;
                } catch (err) {
                    console.error('Failed to toggle favorite', err);
                }
            },
            getBadgeClass(type) {
                const colorMap = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return colorMap[type] || 'bg-teal';
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
    .list-item-card {
        border-radius: 10px;
        height: auto;
    }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .favorite-icon {
        position: absolute;
        top: -9px;
        left: 10px;
        font-size: 1.4rem;
        color: #ffad44c7;
        cursor: pointer;
        z-index: 15;
    }

        .favorite-icon:hover {
            color: #d78418c7;
        }


    .match-score-badge {
        position: absolute;
        top: 25px; /* 14px lower */
        left: 8px;
        background-color: #e7f1ff;
        color: var(--bs-secondary-color) !important;
        font-weight: 600;
        /* padding: 4px;*/
        width: 27px;
        height: 24px;
        display: flex;
        justify-content: center;
        align-items: center;
        border: 1px solid #b6d4fe;
        border-radius: 0.375rem;
        font-size: 0.6rem;
        line-height: 1;
        z-index: 14;
    }

        .match-score-badge small {
            font-size: 1em;
            margin-left: 1px;
        }

    .card-title {
        font-size: 1.1rem;
        font-weight: 500;
        margin-bottom: 0;
    }

    .badge {
        font-size: 0.75rem;
        padding: 0.25em 0.5em;
        color: #fff;
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

    .link-icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }
    .link-icon i {
        font-size: 1.2rem;
        color: #00A89E;
        transition: color 0.15s ease, filter 0.15s ease, text-shadow 0.15s ease;
    }
    .link-icon:hover i {
        color: #007a72; /* darker teal */
        /* subtle glow + drop shadow around the icon only */
        filter: drop-shadow(0 2px 6px rgba(0,0,0,0.25));
        text-shadow: 0 0 6px rgba(0, 168, 158, 0.45);
    }

    .card-body {
        padding: 3px 8px 3px 16px !important;
    }

    .featured-icon {
        color: #FFD700;
        font-size: 0.9rem;
        position: relative;
        top: -3px;
    }
</style>
