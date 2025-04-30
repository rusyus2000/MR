<template>
    <div class="card border-0 shadow-custom h-100">
        <div class="card-body d-flex flex-column p-3">
            <!-- Top bar: asset type badges on left, external link icon on right -->
            <div class="d-flex justify-content-between align-items-center mb-2 nav-icons">
                <div class="asset-types d-flex gap-1">
                    <span v-for="(type, idx) in assetTypes"
                          :key="idx"
                          class="badge"
                          :class="getBadgeClass(type)">
                        {{ type }}
                    </span>
                </div>
                <div>
                    <a v-if="url"
                       :href="url"
                       target="_blank"
                       rel="noopener"
                       class="link-icon"
                       title="Open Resource">
                        <i class="bi bi-box-arrow-up-right"></i>
                    </a>
                </div>
            </div>

            <!-- Clickable title and description to navigate to details page -->
            <router-link v-if="id"
                         :to="{ name: 'ItemDetails', params: { id } }"
                         class="text-decoration-none title-link mb-3">
                <h5 class="card-title mb-1">{{ title }}</h5>
                <p class="card-text text-muted flex-grow-1 mb-0">{{ truncatedDescription }}</p>
            </router-link>
        </div>
    </div>
</template>

<script>
    import { RouterLink } from 'vue-router';
    export default {
        name: 'ItemTile',
        components: { RouterLink },
        props: {
            id: { type: [String, Number], default: null },
            url: { type: String, default: '' },
            title: { type: String, required: true },
            description: { type: String, required: true },
            assetTypes: { type: Array, default: () => [] }
        },
        computed: {
            truncatedDescription() {
                const max = 170;
                return this.description.length <= max
                    ? this.description
                    : this.description.substring(0, max) + '...';
            }
        },
        methods: {
            getBadgeClass(assetType) {
                const colorMap = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return colorMap[assetType] || 'bg-teal';
            }
        }
    };
</script>

<style scoped>
    .card {
        border-radius: 10px;
        min-height: 250px;
        height: 250px;
    }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .asset-types .badge {
        font-size: 0.65rem;
        padding: 0.3em 0.6em;
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

    .title-link {
        cursor: pointer;
    }

    .card-title {
        font-size: 1.25rem;
        margin-bottom: 0.5rem;
    }

    .card-text {
        font-size: 0.9rem;
        line-height: 1.4;
    }
</style>
