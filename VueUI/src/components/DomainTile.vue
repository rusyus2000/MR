<template>
    <router-link :to="{ name: 'DomainItems', params: { domainName: title.toLowerCase().replace(/ & /g, '-').replace(/ /g, '-') } }"
                 class="text-decoration-none h-100">
        <div class="card border-0 shadow-custom h-100">
            <div class="card-body d-flex flex-column">
                <div class="d-flex align-items-center mb-3">
                    <i class="bi bi-globe globe-icon me-2"></i>
                    <h5 class="card-title mb-0">{{ title }}</h5>
                </div>
                <p class="card-text text-muted flex-grow-1">{{ truncatedDescription }}</p>
            </div>
        </div>
    </router-link>
</template>

<script>
    export default {
        name: 'DomainTile',
        props: {
            title: { type: String, required: true },
            description: { type: String, required: true }
        },
        computed: {
            truncatedDescription() {
                const maxChars = 100;
                return this.description.length <= maxChars
                    ? this.description
                    : this.description.substring(0, maxChars) + '...';
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

    .globe-icon {
        font-size: 1.5rem;
        color: #00A89E;
    }
</style>
