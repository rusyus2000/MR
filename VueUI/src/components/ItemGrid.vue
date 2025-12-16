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

                <button v-if="isAdmin" class="btn btn-sm download-btn me-2" title="Download All"
                        @click="exportAll">
                    <i class="bi bi-download"></i>
                </button>

                <button v-if="isAdmin" class="btn btn-sm btn-outline-secondary me-2" title="Upload"
                        @click="openImport">
                    <i class="bi bi-upload"></i>
                </button>

                <button v-if="isAdmin && FEATURE_FLAGS.allowManualAdd" class="btn btn-sm add-asset-btn" title="Add New Asset" @click="openAdd()">
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
        <ModalAddAsset v-if="showAddModal" :edit-item="editItem" @close="closeAdd" @saved="handleAssetSaved" />
        <ModalAssetDetails v-if="selectedItem || detailsLoading" :item="selectedItem" :is-admin="isAdmin" :is-loading="detailsLoading" @edit="openEditFromDetails" @close="closeDetails" />
        <ModalImport v-if="showImportModal" @close="closeImport" @done="$emit('refresh')" />
    </div>
</template>

<script>
    import { ref, computed, watch, toRef, onMounted } from 'vue';
    import ItemTile from './ItemTile.vue';
    import ListItem from './ListItem.vue';
    import ModalAddAsset from './ModalAddAsset.vue';
    import ModalAssetDetails from './ModalAssetDetails.vue';
    import ModalImport from './ModalImport.vue';
    import { favorites } from '../composables/favorites';
    import { fetchItem } from '../services/api';
    import { getCurrentUserCached } from '../services/api';
    import { exportItemsCsv } from '../services/api';
    import { FEATURE_FLAGS } from '../config';

    export default {
        name: 'ItemGrid',
        components: { ItemTile, ListItem, ModalAddAsset, ModalAssetDetails, ModalImport },
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
            const sortBy = ref('Featured');
            const itemsPerPage = ref(15);
            const currentPage = ref(1);
            const showAddModal = ref(false);
            const selectedItem = ref(null);
            const detailsLoading = ref(false);
            const editItem = ref(null);
            const isAdmin = ref(false);
            const searchExecuted = toRef(props, 'searchExecuted');
            const resetSort = (to = 'Most Relevant') => {
                sortBy.value = to;
            };

            const getSort = () => sortBy.value;

            const filteredItems = computed(() =>
                props.items.filter(item => {
                    const f = props.filters;

                    // Helper: give a stringified list for loose comparisons
                    const asStr = arr => (Array.isArray(arr) ? arr.map(x => String(x)) : []);

                    // Asset types: item may have AssetTypeId (number) and AssetTypeName (string)
                    if (f.assetTypes && f.assetTypes.length) {
                        const arr = asStr(f.assetTypes);
                        const matchesId = item.assetTypeId != null && arr.includes(String(item.assetTypeId));
                        const matchesName = arr.includes(String(item.assetTypeName || ''));
                        if (!matchesId && !matchesName) return false;
                    }

                    if (f.privacy && f.privacy.phi && !item.privacyPhi) return false;

                    // Domain
                    if (f.domains && f.domains.length) {
                        const arr = asStr(f.domains);
                        const matchesId = item.domainId != null && arr.includes(String(item.domainId));
                        const matchesName = arr.includes(String(item.domain || ''));
                        if (!matchesId && !matchesName) return false;
                    }

                    // Division
                    if (f.divisions && f.divisions.length) {
                        const arr = asStr(f.divisions);
                        const matchesId = item.divisionId != null && arr.includes(String(item.divisionId));
                        const matchesName = arr.includes(String(item.division || ''));
                        if (!matchesId && !matchesName) return false;
                    }

                    // Service line removed

                    // Data source
                    if (f.dataSources && f.dataSources.length) {
                        const arr = asStr(f.dataSources);
                        const matchesId = item.dataSourceId != null && arr.includes(String(item.dataSourceId));
                        const matchesName = arr.includes(String(item.dataSource || ''));
                        if (!matchesId && !matchesName) return false;
                    }

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
                editItem.value = null;
            };

            const openDetails = async (item) => {
                detailsLoading.value = true;
                selectedItem.value = null;
                try {
                    const full = await fetchItem(item.id);
                    selectedItem.value = full || item;
                } catch (e) {
                    selectedItem.value = item;
                } finally {
                    detailsLoading.value = false;
                }
            };

            const closeDetails = () => {
                selectedItem.value = null;
            };

            const exportAll = async () => {
                try {
                    // No IDs => export all items (admin-only)
                    await exportItemsCsv();
                } catch (e) {
                    console.error('Export failed', e);
                }
            };

            onMounted(async () => {
                const me = await getCurrentUserCached().catch(() => null);
                isAdmin.value = !!me && me.userType === 'Admin';
            });

            const showImportModal = ref(false);

            const openAdd = () => {
                editItem.value = null;
                showAddModal.value = true;
            };

            const openEditFromDetails = () => {
                if (!isAdmin.value) return;
                editItem.value = selectedItem.value;
                selectedItem.value = null;
                showAddModal.value = true;
            };

            const closeAdd = () => {
                showAddModal.value = false;
                editItem.value = null;
            };

            const openImport = () => { showImportModal.value = true; };
            const closeImport = () => { showImportModal.value = false; };

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
                detailsLoading,
                openDetails,
                closeDetails,
                editItem,
                isAdmin,
                openAdd,
                openEditFromDetails,
                closeAdd,
                showImportModal,
                openImport,
                closeImport,
                resetSort,
                getSort,
                filtersActive,
                searchExecuted,
                exportAll,
                FEATURE_FLAGS
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
    .download-btn {
        background-color: #0d6efd !important;
        border-color: #0d6efd !important;
        color: #fff !important;
    }
    .download-btn:hover {
        background-color: #0b5ed7 !important;
        border-color: #0a58ca !important;
    }
    
</style>
