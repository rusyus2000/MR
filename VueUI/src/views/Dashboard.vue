<template>
    <div>
        <Navbar />

        <HeroSection v-model:search="searchTerm"
                     :searching="searching"
                     :searchExecuted="searchExecuted"
                     :searchQuery="searchQuery"
                     @search-submit="runSearch"
                     @clear-search="clearSearch" />

        <div class="content-area d-flex my-4">
            <!-- Fixed 250px sidebar -->
            <FilterSidebar :items-all="allItems"
                           :filters-active="filtersActive"
                           @update:filters="runFilter" />

            <!-- Asset grid/list -->
            <div class="asset-col position-relative">
                <div v-if="searchExecuted" class="mb-3">
                    <span class="search-tag badge d-inline-flex align-items-center">
                        <span class="me-2">Search: <strong>{{ searchQuery }}</strong></span>
                        <button @click="clearSearch" class="btn-close btn-close-sm" aria-label="Close"></button>
                    </span>
                </div>
                <LoadingOverlay v-if="itemsLoading || searching"
                                :message="itemsLoading ? 'Loading reports…' : (searching ? 'Searching…' : '')" />
                <ItemGrid ref="itemGrid"
                          :filters="selectedFilters"
                          :items="items"
                          :search-term="searchTerm"
                          :search-executed="searchExecuted"
                          @refresh="loadItems"
                          @clear-filters="clearFilters" />
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, computed, onMounted } from 'vue';
    import Navbar from '../components/Navbar.vue';
    import HeroSection from '../components/HeroSection.vue';
    import FilterSidebar from '../components/FilterSidebar.vue';
    import ItemGrid from '../components/ItemGrid.vue';
    import LoadingOverlay from '../components/LoadingOverlay.vue';
    import { fetchItems, searchItems, fetchFavorites } from '../services/api';
  
    export default {
        name: 'Dashboard',
        components: { Navbar, HeroSection, FilterSidebar, ItemGrid, LoadingOverlay },
        setup() {

            const filtersActive = computed(() => {
                const f = selectedFilters.value;
                return !!(
                    f.assetTypes.length ||
                    f.privacy.phi ||
                    f.domains.length ||
                    f.divisions.length ||
                    // serviceLines removed
                    f.biPlatforms.length
                );
            });

            const allItems = ref([]);
            const items = ref([]);
            const searchTerm = ref('');
            const searchQuery = ref('');
            const searchExecuted = ref(false);
            const previousSort = ref(null);
            const searching = ref(false);
            const itemGrid = ref(null);
            const itemsLoading = ref(false);

            const selectedFilters = ref({
                assetTypes: [],
                privacy: { phi: false },
                domains: [],
                divisions: [],
                // serviceLines removed
                biPlatforms: []
            });

            const loadItems = async () => {
                itemsLoading.value = true;
                try {
                    const data = await fetchItems();
                    items.value = data || [];
                    allItems.value = data || [];
                } finally {
                    itemsLoading.value = false;
                }
            };

            

            onMounted(() => {
               loadItems();
            });

            async function runSearch(q) {
                if (!q.trim()) return;
                searching.value = true;
                try {
                    console.log('[runSearch]', q);
                    // Save previous sort only if a search is not already active
                    if (!searchExecuted.value) {
                        previousSort.value = itemGrid.value?.getSort ? itemGrid.value.getSort() : 'Favorites';
                    }
                    searchExecuted.value = true;
                    const result = await searchItems(q);
                    items.value = result;
                    console.log('[runSearch] → searchExecuted is now', searchExecuted.value);
                    searchQuery.value = q;
                    // Default sort while search is active should be Most Relevant
                    itemGrid.value?.resetSort('Most Relevant');
                    searchTerm.value = '';
                } finally {
                    searching.value = false;
                }
            }

            function clearSearch() {
                searchExecuted.value = false;
                searchQuery.value = '';
                searchTerm.value = '';
                // Restore the sort that was active prior to the search
                const prev = previousSort.value || 'Favorites';
                itemGrid.value?.resetSort(prev);
                previousSort.value = null;
                loadItems();
            }

            async function runFilter(filters) {
                // Selected filters now contain lookup IDs (not names).
                // If a search has been executed, preserve AI results and apply
                // filters client-side to that result set rather than calling the API.
                const hadActive = filtersActive.value;
                selectedFilters.value = filters;
                if (searchExecuted.value) {
                    // Let ItemGrid filter the current `items` in-memory.
                    // Preserve the current sort (default was set to 'Most Relevant'
                    // when the search executed). Do not override user's explicit
                    // sort choice when filters change while search is active.
                    return;
                }

                // Otherwise request filtered items from the API using ID-based query params.
                try {
                    const flattened = flattenFilters(filters);
                    const isEmpty = !flattened.assetTypeIds && !flattened.domainIds && !flattened.divisionIds && !flattened.serviceLineIds && !flattened.biPlatformIds && !flattened.phi;
                    if (isEmpty) {
                        // If filters were previously active and now cleared, reload full list
                        if (hadActive) {
                            await loadItems();
                        }
                        return;
                    }
                    const qParam = searchQuery.value && searchQuery.value.trim() ? { q: searchQuery.value.trim() } : {};
                    items.value = await fetchItems({
                        ...qParam,
                        ...flattened
                    });
                } catch (err) {
                    console.error('Failed to fetch filtered items', err);
                }
            }

            function clearFilters() {
                selectedFilters.value = {
                    assetTypes: [],
                    privacy: { phi: false },
                    domains: [],
                    divisions: [],
                    serviceLines: [],
                    biPlatforms: []
                };
                // When a search is active, prefer 'Most Relevant' as the default
                // sort; otherwise, fall back to 'Favorites'. Clearing filters
                // then re-running the search will set the sort to Most Relevant
                // within runSearch as well.
                if (searchExecuted.value) {
                    itemGrid.value?.resetSort('Most Relevant');
                } else {
                    itemGrid.value?.resetSort('Favorites');
                }
                runSearch(searchQuery.value);
            }

            function flattenFilters(f) {
                // Build query params using lookup IDs. Use comma-separated lists
                // so the backend can parse them into integer arrays.
                const p = {};
                if (f.assetTypes && f.assetTypes.length) p.assetTypeIds = f.assetTypes.join(',');
                if (f.privacy && f.privacy.phi) p.phi = true;
                if (f.domains && f.domains.length) p.domainIds = f.domains.join(',');
                if (f.divisions && f.divisions.length) p.divisionIds = f.divisions.join(',');
                // serviceLines removed
                if (f.biPlatforms && f.biPlatforms.length) p.biPlatformIds = f.biPlatforms.join(',');
                return p;
            }

            return {
                allItems,
                items,
                searchTerm,
                searchQuery,
                searchExecuted,
                searching,
                itemsLoading,
                selectedFilters,
                runSearch,
                runFilter,
                clearSearch,
                clearFilters,
                loadItems,
                itemGrid,
                filtersActive
            };
        }
    };
</script>

<style scoped>
    .filter-col {
        flex: none !important;
        width: 250px !important;
    }

    .asset-col {
        width: 100%;
        padding: 0 10%;
    }

    .content-area {
        min-height: calc(100vh - 270px);
    }

    .search-tag {
        background-color: #e7f1ff;
        color: #0056b3;
        font-weight: 500;
        padding: 0.4rem 0.6rem;
        border: 1px solid #b6d4fe;
        font-size: 0.85rem;
        border-radius: 0.375rem;
    }

    .btn-close-sm {
        width: 0.75em;
        height: 0.75em;
        font-size: 0.7rem;
    }
</style>
