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
            <div class="asset-col">
                <div v-if="searchExecuted" class="mb-3">
                    <span class="search-tag badge d-inline-flex align-items-center">
                        <span class="me-2">Search: <strong>{{ searchQuery }}</strong></span>
                        <button @click="clearSearch" class="btn-close btn-close-sm" aria-label="Close"></button>
                    </span>
                </div>
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
    import { fetchItems, searchItems, fetchFavorites } from '../services/api';
  
    export default {
        name: 'Dashboard',
        components: { Navbar, HeroSection, FilterSidebar, ItemGrid },
        setup() {

            const filtersActive = computed(() => {
                const f = selectedFilters.value;
                return !!(
                    f.assetTypes.length ||
                    f.privacy.phi ||
                    f.domains.length ||
                    f.divisions.length ||
                    f.serviceLines.length ||
                    f.dataSources.length
                );
            });

            const allItems = ref([]);
            const items = ref([]);
            const searchTerm = ref('');
            const searchQuery = ref('');
            const searchExecuted = ref(false);
            const searching = ref(false);
            const itemGrid = ref(null);

            const selectedFilters = ref({
                assetTypes: [],
                privacy: { phi: false },
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: []
            });

            const loadItems = async () => {
                items.value = await fetchItems();
                allItems.value = items.value;
            };

            

            onMounted(() => {
               loadItems();
            });

            async function runSearch(q) {
                if (!q.trim()) return;
                searching.value = true;
                try {
                    console.log('[runSearch]', q);
                    searchExecuted.value = true;
                    const result = await searchItems(q);
                    items.value = result;
                    console.log('[runSearch] → searchExecuted is now', searchExecuted.value);
                    searchQuery.value = q;
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
                loadItems();
            }

            async function runFilter(filters) {
                selectedFilters.value = filters;
                //items.value = await fetchItems({
                //    q: searchQuery.value,
                //    ...flattenFilters(filters)
                //});
            }

            function clearFilters() {
                selectedFilters.value = {
                    assetTypes: [],
                    privacy: { phi: false },
                    domains: [],
                    divisions: [],
                    serviceLines: [],
                    dataSources: []
                };
                itemGrid.value?.resetSort('Favorites');
                runSearch(searchQuery.value);
            }

            function flattenFilters(f) {
                const p = {};
                if (f.assetTypes.length) p.assetType = f.assetTypes.join(',');
                if (f.privacy.phi) p.phi = true;
                if (f.domains.length) p.domain = f.domains.join(',');
                if (f.divisions.length) p.division = f.divisions.join(',');
                if (f.serviceLines.length) p.serviceLine = f.serviceLines.join(',');
                if (f.dataSources.length) p.dataSource = f.dataSources.join(',');
                return p;
            }

            return {
                allItems,
                items,
                searchTerm,
                searchQuery,
                searchExecuted,
                searching,
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
