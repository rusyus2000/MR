<template>
    <div>
        <Navbar />
        <div class="container my-5">
            <div class="card border-0 shadow-custom">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h2 class="card-title mb-0">{{ item.title }}</h2>
                        <button class="btn btn-outline-secondary" @click="goBack">
                            Back
                        </button>
                    </div>

                    <p class="text-muted mb-4">{{ item.description }}</p>

                    <div class="row mb-4">
                        <div class="col-md-6">
                            <p><strong>Asset Type:</strong> <span class="badge me-1" :class="getBadgeClass(item.assetType)">{{ item.assetType }}</span></p>
                            <p><strong>Domain:</strong> {{ item.domain }}</p>
                            <p><strong>Division:</strong> {{ item.division }}</p>
                            <p><strong>Service Line:</strong> {{ item.serviceLine }}</p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Data Source:</strong> {{ item.dataSource }}</p>
                            <p><strong>PHI:</strong> {{ item.privacy.phi ? 'Yes' : 'No' }}</p>
                            <p v-if="item.createdAt"><strong>Created At:</strong> {{ formattedDate(item.createdAt) }}</p>
                            <p v-if="item.updatedAt"><strong>Updated At:</strong> {{ formattedDate(item.updatedAt) }}</p>
                        </div>
                    </div>

                    <button @click="goToResource" class="btn btn-primary me-2">
                        Go to Resource
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Navbar from '../components/Navbar.vue';
    import itemsData from '../data/itemsData.js';

    export default {
        name: 'ItemDetails',
        components: { Navbar },
        props: {
            id: { type: [String, Number], required: true }
        },
        data() {
            return { item: null };
        },
        created() {
            const found = itemsData.find(i => String(i.id) === String(this.id));
            if (found) this.item = found;
        },
        methods: {
            goToResource() {
                window.open(this.item.url, '_blank');
            },
            goBack() {
                this.$router.back();
            },
            getBadgeClass(type) {
                const map = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured'
                };
                return map[type] || 'bg-teal';
            },
            formattedDate(dt) {
                return new Date(dt).toLocaleString();
            }
        }
    };
</script>

<style scoped>
    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15), 0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .badge {
        color: #fff;
        font-size: 0.85rem;
        padding: 0.4em 0.8em;
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

    .btn-outline-secondary {
        border-color: #e0e0e0;
    }

    .card-title {
        font-size: 1.5rem;
    }
</style>
