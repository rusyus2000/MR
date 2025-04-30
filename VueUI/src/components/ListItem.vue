<template>
    <div class="card list-item-card shadow-custom mb-1">
        <div class="card-body d-flex align-items-center p-2">
            <!-- Title & description -->
            <router-link :to="detailsLink"
                         class="title-link flex-grow-1 text-decoration-none">
                <h5 class="mb-0">{{ title }}</h5>
                <p class="mb-0 text-muted small">{{ truncatedDescription }}</p>
            </router-link>

            <!-- Asset type badge -->
            <span class="badge mx-2" :class="getBadgeClass(assetTypes[0])">
                {{ assetTypes[0] }}
            </span>

            <!-- External link icon -->
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
</template>

<script>
    import { computed } from 'vue';
    export default {
        name: 'ListItem',
        props: {
            id: { type: [String, Number], required: true },
            url: { type: String, default: '' },
            title: { type: String, required: true },
            description: { type: String, required: true },
            assetTypes: { type: Array, default: () => [] }
        },
        setup(props) {
            const detailsLink = computed(() => ({ name: 'ItemDetails', params: { id: props.id } }));
            const getBadgeClass = type => {
                const colorMap = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return colorMap[type] || 'bg-teal';
            };
            const truncatedDescription = computed(() => {
                const max = 60;
                return props.description.length > max ? props.description.slice(0, max) + '...' : props.description;
            });
            return { detailsLink, getBadgeClass, truncatedDescription };
        }
    };
</script>

<style scoped>
    .list-item-card {
        border-radius: 10px;
        height: auto;
    }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15),0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .title-link h5 {
        margin: 0;
        font-size: 1.1rem;
    }

    .title-link p {
        margin: 0;
        font-size: 0.85rem;
    }

    .badge {
        font-size: 0.75rem;
        padding: 0.25em 0.5em;
        color: #fff;
    }

    /* Badge background colors */
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
        padding: 7px 8px 7px 16px !important; /* increased left padding */
    }
</style>
