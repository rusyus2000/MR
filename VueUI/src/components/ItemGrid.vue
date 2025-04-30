<template>
    <div>
        <!-- Header: count, sort, add asset -->
        <div class="d-flex justify-content-between align-items-center mb-3">
            <span class="text-muted">
                Showing {{ paginatedItems.length }} of {{ filteredItems.length }} of {{ items.length }} results
            </span>
            <div class="d-flex align-items-center">
                <label for="sortBy" class="me-2 mb-0">Sort by:</label>
                <select id="sortBy" class="form-select form-select-sm me-3" v-model="sortBy" style="width: auto;">
                    <option>Most Relevant</option>
                    <option>Alphabetical</option>
                </select>
                <router-link to="/add-asset" class="btn btn-sm btn-success">
                    <i class="bi bi-plus"></i> Add New Asset
                </router-link>
            </div>
        </div>

        <!-- View toggle controls -->
        <div class="d-flex justify-content-end mb-3">
            <button class="btn btn-sm me-2"
                    :class="{ 'btn-primary': viewMode==='grid', 'btn-outline-secondary': viewMode!=='grid' }"
                    @click="viewMode='grid'"
                    title="Grid View">
                <i class="bi bi-grid"></i>
            </button>
            <button class="btn btn-sm"
                    :class="{ 'btn-primary': viewMode==='list', 'btn-outline-secondary': viewMode!=='list' }"
                    @click="viewMode='list'"
                    title="List View">
                <i class="bi bi-list"></i>
            </button>
        </div>

        <!-- Grid View -->
        <div v-if="viewMode==='grid'" class="row row-cols-1 row-cols-md-3 g-4">
            <div class="col" v-for="item in paginatedItems" :key="item.id">
                <ItemTile :id="item.id"
                          :url="item.url"
                          :title="item.title"
                          :description="item.description"
                          :asset-types="item.assetTypes" />
            </div>
        </div>

        <!-- List View -->
        <div v-else class="list-view">
            <ListItem v-for="item in paginatedItems"
                      :key="item.id"
                      :id="item.id"
                      :url="item.url"
                      :title="item.title"
                      :description="item.description"
                      :asset-types="item.assetTypes" />
        </div>

        <!-- Pagination Controls -->
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
                {{ (currentPage-1)*itemsPerPage+1 }}-{{ Math.min(currentPage*itemsPerPage, filteredItems.length) }} of {{ filteredItems.length }}
            </div>
            <div>
                <button class="btn btn-sm btn-outline-secondary me-1" :disabled="currentPage===1" @click="currentPage--">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage===totalPages" @click="currentPage++">
                    <i class="bi bi-chevron-right"></i>
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, computed, watch } from 'vue';
    import ItemTile from '../components/ItemTile.vue';
    import ListItem from '../components/ListItem.vue';

    export default {
        name: 'ItemGrid',
        components: { ItemTile, ListItem },
        props: {
            filters: { type: Object, default: () => ({ assetTypes: [], privacy: { phi: false }, domains: [], divisions: [], serviceLines: [], dataSources: [] }) },
            items: { type: Array, default: () => [] }
        },
        setup(props) {
            const viewMode = ref('grid');
            const sortBy = ref('Most Relevant');
            const itemsPerPage = ref(15);
            const currentPage = ref(1);

            const filteredItems = computed(() =>
                props.items.filter(item => {
                    if (props.filters.assetTypes.length && !props.filters.assetTypes.some(type => item.assetTypes.includes(type))) return false;
                    if (props.filters.privacy.phi && !item.privacy.phi) return false;
                    if (props.filters.domains.length && !props.filters.domains.includes(item.domain)) return false;
                    if (props.filters.divisions.length && !props.filters.divisions.includes(item.division)) return false;
                    if (props.filters.serviceLines.length && !props.filters.serviceLines.includes(item.serviceLine)) return false;
                    if (props.filters.dataSources.length && !props.filters.dataSources.includes(item.dataSource)) return false;
                    return true;
                })
            );

            const totalPages = computed(() => Math.ceil(filteredItems.value.length / itemsPerPage.value));

            const paginatedItems = computed(() => {
                let list = filteredItems.value;
                if (sortBy.value === 'Alphabetical') list = [...list].sort((a, b) => a.title.localeCompare(b.title));
                const start = (currentPage.value - 1) * itemsPerPage.value;
                return list.slice(start, start + itemsPerPage.value);
            });

            watch(filteredItems, () => {
                currentPage.value = 1;
            });

            return { viewMode, sortBy, itemsPerPage, currentPage, filteredItems, totalPages, paginatedItems };
        }
    };
</script>

<style scoped>
    .list-view {
        display: flex;
        flex-direction: column;
        gap: 5px;
        padding: 0 5px;
    }
</style>
