<template>
    <div>
        <Navbar />

        <div class="container my-5" style="max-width: 800px;">
            <button class="btn btn-link mb-3" @click="goBack">
                ← Back to List
            </button>

            <div v-if="loading" class="text-center my-5">
                <div class="spinner-border" role="status"></div>
            </div>

            <div v-else-if="error" class="alert alert-danger">
                {{ error }}
            </div>

            <div v-else class="card shadow-custom">
                <div class="card-body">
                    <h2 class="card-title mb-3">{{ item.title }}</h2>
                    <p class="text-muted">{{ item.description }}</p>

                    <dl class="row">
                        <dt class="col-sm-4">URL</dt>
                        <dd class="col-sm-8">
                            <a :href="item.url" target="_blank">{{ item.url }}</a>
                        </dd>

                        <dt class="col-sm-4">Asset Types</dt>
                        <dd class="col-sm-8">
                            <span v-for="(type, idx) in item.assetTypes"
                                  :key="idx"
                                  :class="['badge me-1', getBadgeClass(type)]">
                                {{ type }}
                            </span>
                        </dd>

                        <dt class="col-sm-4">Domain</dt>
                        <dd class="col-sm-8">{{ item.domain }}</dd>

                        <dt class="col-sm-4">Division</dt>
                        <dd class="col-sm-8">{{ item.division }}</dd>

                        <dt class="col-sm-4">Service Line</dt>
                        <dd class="col-sm-8">{{ item.serviceLine }}</dd>

                        <dt class="col-sm-4">Data Source</dt>
                        <dd class="col-sm-8">{{ item.dataSource }}</dd>

                        <dt class="col-sm-4">Contains PHI?</dt>
                        <dd class="col-sm-8">
                            <span v-if="item.privacyPhi" class="text-danger">Yes</span>
                            <span v-else>No</span>
                        </dd>

                        <dt class="col-sm-4">Date Added</dt>
                        <dd class="col-sm-8">{{ formattedDate }}</dd>
                    </dl>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, onMounted, computed } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import Navbar from '../components/Navbar.vue';
    import { fetchItem } from '../services/api';

    export default {
        name: 'ItemDetails',
        components: { Navbar },
        setup() {
            const route = useRoute();
            const router = useRouter();
            const id = route.params.id;

            const item = ref(null);
            const loading = ref(true);
            const error = ref('');

            onMounted(async () => {
                try {
                    item.value = await fetchItem(id);
                } catch (err) {
                    error.value = err.message;
                } finally {
                    loading.value = false;
                }
            });

            function goBack() {
                router.back();
            }

            function getBadgeClass(type) {
                const map = {
                    Dashboard: 'bg-dashboard',
                    Report: 'bg-report',
                    Application: 'bg-application',
                    'Data Model': 'bg-data-model',
                    Featured: 'bg-featured',
                };
                return map[type] || 'bg-teal';
            }

            const formattedDate = computed(() => {
                if (!item.value) return '';
                const d = new Date(item.value.dateAdded);
                return d.toLocaleString();
            });

            return { item, loading, error, goBack, getBadgeClass, formattedDate };
        },
    };
</script>

<style scoped>
    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0,0,0,0.15), 0 6px 6px rgba(0,0,0,0.1);
    }

    .badge {
        color: #fff;
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
</style>
