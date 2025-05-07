<template>
    <div class="card list-item-card shadow-custom mb-1">
        <div class="card-body d-flex align-items-center p-2 position-relative">
            <!-- Bookmark Favorite Icon -->
            <i class="bi favorite-icon"
               :class="isFavorite ? 'bi-bookmark-fill' : 'bi-bookmark'"
               :title="isFavorite ? 'Remove from favorites' : 'Add to favorites'"
               @click.stop="toggleFavorite"></i>

            <!-- Title & description -->
            <div class="flex-grow-1 ps-4">
                <a href="#" @click.prevent="$emit('show-details')" class="text-decoration-none">
                    <h5 class="mb-0 card-title">{{ item.title }}</h5>
                </a>
                <p class="mb-0 text-muted small">{{ truncatedDescription }}</p>
            </div>

            <!-- Asset type badge -->
            <span class="badge mx-2" :class="getBadgeClass(item.assetTypes[0])">
                {{ item.assetTypes[0] }}
            </span>

            <!-- External link icon -->
            <a v-if="item.url"
               :href="item.url"
               target="_blank"
               rel="noopener"
               class="link-icon"
               title="Open Resource">
                <i class="bi bi-box-arrow-up-right"></i>
            </a>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'ListItem',
        props: {
            item: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                isFavorite: false
            };
        },
        computed: {
            truncatedDescription() {
                const max = 90;
                return this.item.description.length > max
                    ? this.item.description.slice(0, max) + '...'
                    : this.item.description;
            }
        },
        mounted() {
            const favs = JSON.parse(localStorage.getItem('favorites') || '[]');
            this.isFavorite = favs.includes(this.item.id);
        },
        methods: {
            getBadgeClass(type) {
                const colorMap = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return colorMap[type] || 'bg-teal';
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

    .link-icon i {
        font-size: 1.2rem;
        color: #00A89E;
    }

    .card-body {
        padding: 7px 8px 7px 16px !important;
    }
</style>
