<template>
    <div class="item-grid-container my-4">
        <!-- Header -->
        <div class="d-flex justify-content-between align-items-center mb-3">
            <span class="text-muted">
                Showing {{ paginatedItems.length }} of {{ filteredItems.length }} of {{ items.length }} results
            </span>
            <div class="d-flex align-items-center">
                <label for="sortBy" class="me-2 mb-0">Sort by:</label>
                <select id="sortBy"
                        class="form-select form-select-sm me-3"
                        v-model="sortBy"
                        style="width: auto;">
                    <option>Favorites</option>
                    <option>Featured</option>
                    <option>Most Relevant</option>
                    <option>Alphabetical</option>
                </select>

                <button class="btn btn-sm me-2"
                        :class="{ 'btn-primary': viewMode==='grid', 'btn-outline-secondary': viewMode!=='grid' }"
                        @click="viewMode='grid'"
                        title="Grid View">
                    <i class="bi bi-grid"></i>
                </button>
                <button class="btn btn-sm me-3"
                        :class="{ 'btn-primary': viewMode==='list', 'btn-outline-secondary': viewMode!=='list' }"
                        @click="viewMode='list'"
                        title="List View">
                    <i class="bi bi-list"></i>
                </button>

                <button class="btn btn-sm add-asset-btn" title="Add New Asset" @click="showAddModal = true">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
        </div>

        <!-- Grid View -->
        <div v-if="viewMode==='grid'" class="grid-wrapper">
            <div class="row row-cols-1 row-cols-md-3 g-4">
                <div class="col" v-for="item in paginatedItems" :key="item.id">
                    <ItemTile :key="item.id + '-' + (searchExecuted ? 'yes' : 'no')"
                              :item="item"
                              :search-executed="searchExecuted"
                              @show-details="openDetails(item)"
                              @refresh="handleAssetSaved" />
                </div>
            </div>
        </div>

        <!-- List View -->
        <div v-else class="list-wrapper">
            <ListItem v-for="item in paginatedItems"
                      :key="item.id"
                      :item="item"
                       :search-executed="searchExecuted"
                      @show-details="openDetails(item)"
                      @refresh="handleAssetSaved" />
        </div>

        <!-- Pagination Controls -->
        <div class="d-flex justify-content-between align-items-center mt-4">
            <div class="d-flex align-items-center">
                <label for="itemsPerPage" class="me-2">Items per page</label>
                <select id="itemsPerPage"
                        class="form-select form-select-sm"
                        v-model="itemsPerPage"
                        style="width: auto;">
                    <option>15</option>
                    <option>30</option>
                    <option>50</option>
                </select>
            </div>
            <div class="text-muted">
                {{ (currentPage - 1) * itemsPerPage + 1 }}-
                {{ Math.min(currentPage * itemsPerPage, filteredItems.length) }}
                of {{ filteredItems.length }}
            </div>
            <div>
                <button class="btn btn-sm btn-outline-secondary me-1"
                        :disabled="currentPage === 1"
                        @click="currentPage--">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary"
                        :disabled="currentPage === totalPages"
                        @click="currentPage++">
                    <i class="bi bi-chevron-right"></i>
                </button>
            </div>
        </div>

        <!-- Modals -->
        <ModalAddAsset v-if="showAddModal" @close="showAddModal = false" @saved="handleAssetSaved" />
        <ModalAssetDetails v-if="selectedItem" :item="selectedItem" @close="selectedItem = null" />
    </div>
</template>

<script>
    import { ref, computed, watch, toRef } from 'vue';
    import ItemTile from './ItemTile.vue';
    import ListItem from './ListItem.vue';
    import ModalAddAsset from './ModalAddAsset.vue';
    import ModalAssetDetails from './ModalAssetDetails.vue';
    import { favorites } from '../composables/favorites';

    export default {
        name: 'ItemGrid',
        components: { ItemTile, ListItem, ModalAddAsset, ModalAssetDetails },
        props: {
            filters: {
                type: Object,
                default: () => ({
                    assetTypes: [],
                    privacy: { phi: false },
                    domains: [],
                    divisions: [],
                    serviceLines: [],
                    dataSources: [],
                }),
            },
            items: {
                type: Array,
                default: () => [],
            },
            searchExecuted: {            
                type: Boolean,
                default: false
            },
        },
        setup(props, { emit }) {
            const viewMode = ref('grid');
            const sortBy = ref('Favorites');
            const itemsPerPage = ref(15);
            const currentPage = ref(1);
            const showAddModal = ref(false);
            const selectedItem = ref(null);
            const searchExecuted = toRef(props, 'searchExecuted');
            const resetSort = (to = 'Most Relevant') => {
                sortBy.value = to;
            };

            const filteredItems = computed(() =>
                props.items.filter(item => {
                    const f = props.filters;
                    if (f.assetTypes.length && !f.assetTypes.some(type => (item.assetTypeName || '') === type)) return false;
                    if (f.privacy.phi && !item.privacyPhi) return false;
                    if (f.domains.length && !f.domains.includes(item.domain)) return false;
                    if (f.divisions.length && !f.divisions.includes(item.division)) return false;
                    if (f.serviceLines.length && !f.serviceLines.includes(item.serviceLine)) return false;
                    if (f.dataSources.length && !f.dataSources.includes(item.dataSource)) return false;
                    return true;
                })
            );

            const totalPages = computed(() =>
                Math.ceil(filteredItems.value.length / itemsPerPage.value)
            );

            const paginatedItems = computed(() => {
                let list = filteredItems.value;

                if (sortBy.value === 'Alphabetical') {
                    list = [...list].sort((a, b) => a.title.localeCompare(b.title));
                } else if (sortBy.value === 'Featured') {
                    list = [...list].sort((a, b) => {
                        const aFeat = a.featured ? 0 : 1;
                        const bFeat = b.featured ? 0 : 1;
                        return aFeat - bFeat; // featured first
                    });
                } else if (sortBy.value === 'Favorites') {
                    list = [...list].sort((a, b) => {
                        const aFav = a.isFavorite ? 0 : 1;
                        const bFav = b.isFavorite ? 0 : 1;
                        return aFav - bFav; // favorites first
                    });
                }

                const start = (currentPage.value - 1) * itemsPerPage.value;
                return list.slice(start, start + itemsPerPage.value);
            });

            watch([filteredItems, sortBy], () => {
                currentPage.value = 1;
            });

            watch(() => props.searchExecuted, (val) => {
                console.log('[ItemGrid] searchExecuted changed to:', val);
            });

            const handleAssetSaved = () => {
                emit('refresh');
                showAddModal.value = false;
            };

            const openDetails = (item) => {
                selectedItem.value = item;
            };

            const filtersActive = computed(() => filteredItems.value.length < props.items.length);

            return {
                viewMode,
                sortBy,
                itemsPerPage,
                currentPage,
                filteredItems,
                totalPages,
                paginatedItems,
                showAddModal,
                handleAssetSaved,
                selectedItem,
                openDetails,
                resetSort,
                filtersActive,
                searchExecuted
            };
        }
    };
</script>

<style scoped>
    .item-grid-container {
        margin: 1rem 0;
    }

    .grid-wrapper,
    .list-wrapper {
        width: 100%;
        max-width: 1200px;
        margin: 0 auto;
    }

    .add-asset-btn {
        background-color: #28a745 !important;
        border-color: #28a745 !important;
        color: #fff !important;
    }

        .add-asset-btn:hover {
            background-color: #218838 !important;
            border-color: #1e7e34 !important;
        }
    
</style>
