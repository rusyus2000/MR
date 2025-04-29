<template>
    <div class="item-grid">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <span class="text-muted">
                Showing {{ paginatedItems.length }} of {{ filteredItems.length }} of {{ items.length }} results
            </span>
            <div class="d-flex align-items-center">
                <label for="sortBy" class="me-2">Sort by:</label>
                <select id="sortBy" class="form-select form-select-sm" v-model="sortBy">
                    <option>Most Relevant</option>
                    <option>Alphabetical</option>
                </select>
                <button class="btn btn-sm btn-outline-secondary ms-2">
                    <i class="bi bi-grid"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary ms-1">
                    <i class="bi bi-list"></i>
                </button>
            </div>
        </div>

        <div class="row row-cols-1 row-cols-md-3 g-4">
            <div class="col" v-for="item in paginatedItems" :key="item.id">
                <ItemTile :id="item.id"
                          :url="item.url"
                          :title="item.title"
                          :description="item.description"
                          :asset-types="item.assetTypes"
                          :show-request-access="true"
                          :has-access="item.hasAccess" />
            </div>
        </div>

        <div class="d-flex justify-content-between align-items-center mt-4">
            <div class="d-flex align-items-center">
                <label for="itemsPerPage" class="me-2">Items per page</label>
                <select id="itemsPerPage" class="form-select form-select-sm" v-model="itemsPerPage" style="width: auto;">
                    <option>15</option>
                    <option>30</option>
                    <option>50</option>
                </select>
            </div>
            <div class="text-muted">
                {{ (currentPage - 1) * itemsPerPage + 1 }}-{{ Math.min(currentPage * itemsPerPage, filteredItems.length) }} of {{ filteredItems.length }}
            </div>
            <div>
                <button class="btn btn-sm btn-outline-secondary me-1" :disabled="currentPage === 1" @click="currentPage--">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === totalPages" @click="currentPage++">
                    <i class="bi bi-chevron-right"></i>
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import ItemTile from '../components/ItemTile.vue';

    export default {
        name: 'ItemGrid',
        components: { ItemTile },
        props: {
            filters: {
                type: Object,
                default: () => ({
                    assetTypes: [],
                    privacy: { phi: false },
                    domains: [],
                    divisions: [],
                    serviceLines: [],
                    dataSources: []
                })
            },
            items: {
                type: Array,
                default: () => []
            }
        },
        data() {
            return {
                sortBy: 'Most Relevant',
                itemsPerPage: 15,
                currentPage: 1
            };
        },
        computed: {
            filteredItems() {
                return this.items.filter(item => {
                    if (this.filters.assetTypes.length > 0) {
                        const match = this.filters.assetTypes.some(type => item.assetTypes.includes(type));
                        if (!match) return false;
                    }
                    if (this.filters.privacy.phi && !item.privacy.phi) return false;
                    if (this.filters.domains.length > 0 && !this.filters.domains.includes(item.domain)) return false;
                    if (this.filters.divisions.length > 0 && !this.filters.divisions.includes(item.division)) return false;
                    if (this.filters.serviceLines.length > 0 && !this.filters.serviceLines.includes(item.serviceLine)) return false;
                    if (this.filters.dataSources.length > 0 && !this.filters.dataSources.includes(item.dataSource)) return false;
                    return true;
                });
            },
            totalPages() {
                return Math.ceil(this.filteredItems.length / this.itemsPerPage);
            },
            paginatedItems() {
                let list = this.filteredItems;
                if (this.sortBy === 'Alphabetical') {
                    list = [...list].sort((a, b) => a.title.localeCompare(b.title));
                }
                const start = (this.currentPage - 1) * this.itemsPerPage;
                return list.slice(start, start + this.itemsPerPage);
            }
        },
        watch: {
            filteredItems() {
                this.currentPage = 1;
            }
        }
    };
</script>

<style scoped>
    .item-grid {
        padding: 20px;
    }

    .form-select-sm {
        width: auto;
    }

    .btn-outline-secondary {
        border-color: #e0e0e0;
    }

    .text-muted {
        font-size: 0.9rem;
    }
</style>
