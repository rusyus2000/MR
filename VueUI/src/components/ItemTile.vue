<template>
    <div class="card border-0 shadow-custom">
        <div class="card-body position-relative">
            <!-- Bookmark Favorite Icon -->
            <i class="bi favorite-icon"
               :class="isFavorite ? 'bi-bookmark-fill' : 'bi-bookmark'"
               :title="isFavorite ? 'Remove from favorites' : 'Add to favorites'"
               @click.stop="toggleFavorite"></i>

            <!-- Asset type badges top-right -->
            <div class="asset-type-wrapper">
                <span v-for="(type, idx) in item.assetTypes"
                      :key="idx"
                      :class="['badge', getBadgeClass(type), { 'me-1': idx < item.assetTypes.length - 1 }]">
                    {{ type }}
                </span>
            </div>

            <!-- Title -->
            <a href="#" class="title-link d-block text-decoration-none mb-2 mt-1" @click.prevent="$emit('show-details')">
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

<script>
    export default {
        name: 'ItemTile',
        props: {
            item: {
                type: Object,
                required: true,
            },
        },
        data() {
            return {
                isFavorite: false,
            };
        },
        mounted() {
            const favs = JSON.parse(localStorage.getItem('favorites') || '[]');
            this.isFavorite = favs.includes(this.item.id);
        },
        methods: {
            getBadgeClass(assetType) {
                const map = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured',
                };
                return map[assetType] || 'bg-teal';
            },
            toggleFavorite() {
                const favs = new Set(JSON.parse(localStorage.getItem('favorites') || '[]'));
                if (favs.has(this.item.id)) {
                    favs.delete(this.item.id);
                    this.isFavorite = false;
                } else {
                    favs.add(this.item.id);
                    this.isFavorite = true;
                }
                localStorage.setItem('favorites', JSON.stringify([...favs]));
                this.$emit('refresh');
            }
        },
    };
</script>

<style scoped>
    .favorite-icon {
        position: absolute;
        top: -10px;
        left: 10px;
        font-size: 1.4rem;
        color: #339999;
        cursor: pointer;
        z-index: 15;
    }

        .favorite-icon:hover {
            color: #227777;
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
