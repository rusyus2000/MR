<template>
    <div>
        <Navbar />

        <HeroSection v-model:search="searchTerm"
                     @search-submit="runSearch" />

        <div class="content-area d-flex my-4">
            <!-- Fixed 250px sidebar -->
            <div class="filter-col">
                <FilterSidebar :items-all="allItems"
                               @update:filters="runFilter" />
            </div>

            <!-- Asset grid/list -->
            <div class="asset-col">
                <ItemGrid :filters="selectedFilters"
                          :items="items"
                          :search-term="searchTerm" />
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, onMounted } from 'vue';
    import Navbar from '../components/Navbar.vue';
    import HeroSection from '../components/HeroSection.vue';
    import FilterSidebar from '../components/FilterSidebar.vue';
    import ItemGrid from '../components/ItemGrid.vue';
    import { fetchItems } from '../services/api';

    export default {
        name: 'Dashboard',
        components: { Navbar, HeroSection, FilterSidebar, ItemGrid },
        setup() {
            const allItems = ref([]);
            const items = ref([]);
            const searchTerm = ref('');
            const selectedFilters = ref({
                assetTypes: [], privacy: { phi: false },
                domains: [], divisions: [], serviceLines: [], dataSources: []
            });

            // On mount, load full list and display it
            onMounted(async () => {
                allItems.value = await fetchItems();
                items.value = allItems.value;
            });

            // Called when user presses Enter in the search box
            async function runSearch(q) {
                items.value = await fetchItems({
                    q,
                    ...flattenFilters(selectedFilters.value)
                });
            }

            // Called whenever filters change
            async function runFilter(filters) {
                selectedFilters.value = filters;
                items.value = await fetchItems({
                    q: searchTerm.value,
                    ...flattenFilters(filters)
                });
            }

            // Helper to build API query params from the filters object
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
                selectedFilters,
                runSearch,
                runFilter,
            };
        },
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
</style>
