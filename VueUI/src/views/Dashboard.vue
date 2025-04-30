<template>
    <div>
        <Navbar />

        <HeroSection v-model:search="searchTerm" />

        <div class="content-area d-flex my-4">
            <!-- Filters column (fixed width) -->
            <div class="filter-col">
                <FilterSidebar :items="items"
                               :current-domain="''"
                               @update:filters="onFilterChange"
                               :show-domain="true" />
            </div>

            <!-- Results column now fills 100% width -->
            <div class="asset-col">
                <ItemGrid :filters="selectedFilters" :items="items" />
            </div>
        </div>
    </div>
</template>

<script>
    import { ref, watch, onMounted } from 'vue';
    import Navbar from '../components/Navbar.vue';
    import HeroSection from '../components/HeroSection.vue';
    import FilterSidebar from '../components/FilterSidebar.vue';
    import ItemGrid from '../components/ItemGrid.vue';
    import { fetchItems } from '../services/api';

    export default {
        name: 'Dashboard',
        components: { Navbar, HeroSection, FilterSidebar, ItemGrid },
        setup() {
            const items = ref([]);
            const searchTerm = ref('');
            const selectedFilters = ref({
                assetTypes: [],
                privacy: { phi: false },
                domains: [],
                divisions: [],
                serviceLines: [],
                dataSources: [],
            });

            onMounted(() => loadItems());

            watch(searchTerm, val => {
                selectedFilters.value = {
                    assetTypes: [], privacy: { phi: false },
                    domains: [], divisions: [], serviceLines: [], dataSources: []
                };
                loadItems({ q: val });
            });

            function onFilterChange(filters) {
                searchTerm.value = '';
                selectedFilters.value = filters;
                loadItems(flattenFilters(filters));
            }

            function flattenFilters(f) {
                const params = {};
                if (f.assetTypes.length) params.assetType = f.assetTypes.join(',');
                if (f.privacy.phi) params.phi = true;
                if (f.domains.length) params.domain = f.domains.join(',');
                if (f.divisions.length) params.division = f.divisions.join(',');
                if (f.serviceLines.length) params.serviceLine = f.serviceLines.join(',');
                if (f.dataSources.length) params.dataSource = f.dataSources.join(',');
                return params;
            }

            async function loadItems(params = {}) {
                items.value = await fetchItems(params);
            }

            return { items, searchTerm, selectedFilters, onFilterChange };
        },
    };
</script>

<style scoped>
    .filter-col {
        width: 250px; /* fixed width for sidebar */
    }

    .asset-col {
        width: 100%; /* now spans the full remaining width */
        padding: 0 10%;
        justify-content: center;
    }

    .content-area {
        min-height: calc(100vh - 270px);
    }
</style>
